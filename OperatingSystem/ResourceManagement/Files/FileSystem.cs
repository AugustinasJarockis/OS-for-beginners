using System.Runtime.InteropServices;
using System.Text;
using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.ProcessManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using Serilog;

namespace OperatingSystem.ResourceManagement.Files;

public static class FileSystem
{
    private const uint BlockCount = SizeConstants.EXTERNAL_STORAGE_SIZE / 4;
    
    private static ExternalStorage _externalStorage;
    private static ProcessManager _processManager;
    private static ResourceManager _resourceManager;
    private static ExternalStorageBlockMetadata[] _blocksMetadata;
    private static Dictionary<string, (uint, List<ExternalStorageBlockMetadata>)> _blocksByFileName = new();

    private static readonly uint[] EmptyBlock = new uint[ExternalStorage.BLOCK_SIZE];

    public static void Initialise(ExternalStorage externalStorage, ProcessManager processManager, ResourceManager resourceManager)
    {
        _externalStorage = externalStorage;
        _processManager = processManager;
        _resourceManager = resourceManager;
        _blocksMetadata = new ExternalStorageBlockMetadata[BlockCount];

        for (var i = 0; i < BlockCount; i++)
        {
            _blocksMetadata[i] = new ExternalStorageBlockMetadata
            {
                BlockIndex = (uint)i,
                AllocatedToPid = null
            };
        }
    }

    public static bool FileExists(string fileName) {
        return _blocksByFileName.ContainsKey(fileName);
    }

    public static List<string> GetFileList() {
        return _blocksByFileName.Keys.ToList();
    }
    
    public static FileHandleData? CreateFile(string fileName)
    {
        if (_blocksByFileName.ContainsKey(fileName))
        {
            return null;
        }

        _blocksByFileName.Add(fileName, (0, []));
        var fileHandle = new FileHandleData
        {
            Name = fileName,
            IsSingleUse = false,
        };
        _resourceManager.AddResourcePart(ResourceNames.FileHandle, fileHandle);
        
        Log.Information("File {FileName} created by pid {Pid}", fileName, _processManager.CurrentProcessId);
        return fileHandle;
    }
    public static bool DeleteFile(FileHandleData fileHandle) 
    { 
        if (!_blocksByFileName.TryGetValue(fileHandle.Name, out var fileData)) {
            return false;
        }
        var blocks = fileData.Item2;

        foreach (var block in blocks) {
            _externalStorage.WriteBlock(block.BlockIndex, EmptyBlock);
            block.AllocatedToPid = null;
        }

        _blocksByFileName.Remove(fileHandle.Name);

        Log.Information("File {FileName} deleted by pid {Pid}", fileHandle.Name, fileHandle.GrantedToPid);
        return true;
    }

    public static void OverwriteFile(FileHandleData fileHandle, string[] content) {
        var joinedContent = string.Join(Environment.NewLine, content);
        byte[] byteContent = Encoding.UTF8.GetBytes(joinedContent);
        OverwriteFile(fileHandle, byteContent);
    }
    public static void OverwriteFile(FileHandleData fileHandle, byte[] content)
    {
        var currentProcessId = _processManager.CurrentProcessId;
        Log.Information("Overwriting file {FileName} by pid {Pid}", fileHandle.Name, currentProcessId);

        DeleteFile(fileHandle);

        // TODO: here should be a call to scheduler, but lets assume that we always have enough external storage :)

        uint fileSize = (uint)content.Length;
        var blocksToWrite = SplitToBlocks(content);
        List<ExternalStorageBlockMetadata> blocksAllocatedToRequester = [];
        for (var i = 0; i < blocksToWrite.Length; i++)
        {
            var unusedBlock = _blocksMetadata.FirstOrDefault(b => b.AllocatedToPid is null && !blocksAllocatedToRequester.Contains(b));
            if (unusedBlock is not null)
            {
                blocksAllocatedToRequester.Add(unusedBlock);
            }
        }

        if (blocksAllocatedToRequester.Count != blocksToWrite.Length)
        {
            return;
        }
        
        for (var i = 0; i < blocksAllocatedToRequester.Count; i++)
        {
            var block = blocksAllocatedToRequester[i];
            block.AllocatedToPid = currentProcessId;
            var data = ByteArrayToUIntArray(blocksToWrite[i]);
            _externalStorage.WriteBlock(block.BlockIndex, data);
        }

        _blocksByFileName[fileHandle.Name] = (fileSize, blocksAllocatedToRequester);
        
        Log.Information("Allocated {BlockCount} external storage blocks to file {FileName} for pid {Pid}", blocksAllocatedToRequester.Count, fileHandle.Name, currentProcessId);
    }

    public static string[]? ReadFileString(FileHandleData fileHandle, long symbolsToRead = long.MaxValue, long bytesToSkip = 0) {
        var byteContent = ReadFile(fileHandle, symbolsToRead, bytesToSkip);

        if (byteContent == null)
            return null;

        string stringContent = Encoding.UTF8.GetString(byteContent, 0, byteContent.Length);
        string[] stringArray = stringContent.Split('\n');
        return stringArray;
    }
    public static byte[]? ReadFile(FileHandleData fileHandle, long symbolsToRead = long.MaxValue, long bytesToSkip = 0)
    {
        Log.Information("Reading from file {FileName}", fileHandle.Name);

        if (!_blocksByFileName.TryGetValue(fileHandle.Name, out var fileData))
        {
            return null;
        }

        var fileSize = fileData.Item1;
        var blocksMetadata = fileData.Item2;

        uint bytesToRead = (uint)Math.Min(fileSize - bytesToSkip, symbolsToRead);
        byte[] content = new byte[bytesToRead];


        uint currentBlockNr = (uint)(bytesToSkip / 4096);
        uint[] blockIndexes = blocksMetadata.Select(m => m.BlockIndex).ToArray();

        if (blockIndexes.Length == 0)
            return [];

        var currentBlock = _externalStorage.ReadBlock(blockIndexes[currentBlockNr]);

        for (int i = (int)bytesToSkip; i < bytesToRead + bytesToSkip; i++) {
            if (i % 4096 == 0) {
                currentBlockNr = (uint)(i / 4096);
                currentBlock = _externalStorage.ReadBlock(blockIndexes[currentBlockNr]);
            }

            int shiftAmount = 8 * (3 - (i % 4));
            var val = (byte)((currentBlock[i / 4] & (0xFF << shiftAmount)) >> shiftAmount);

            content[i - bytesToSkip] = val;
        }

        return content;
    }

    private static string[] SplitToBlocks(string[] content)
    {
        var joinedContent = string.Join(Environment.NewLine, content);
        return joinedContent.Chunk(4096 / sizeof(char)).Select(c => new string(c)).ToArray();
    }

    private static byte[][] SplitToBlocks(byte[] content) {
        return [.. content.Chunk(4096 / sizeof(byte))];
    }

    private static uint[] StringToUIntArray(string str)
    {
        List<uint> data = [];
        uint part = 0;
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            if (i % 2 == 0)
            {
                part |= (uint)ch << 16;
                if (i == str.Length - 1)
                {
                    data.Add(part);
                }
            }
            else
            {
                part |= ch;
                data.Add(part);
                part = 0;
            }
        }

        return data.ToArray();
    }

    private static uint[] ByteArrayToUIntArray(byte[] buffer) {
        List<uint> data = [];
        uint part = 0;
        for (var i = 0; i < buffer.Length; i++) {
            var val = buffer[i];
            if (i % 4 == 0) {
                part |= (uint)val << 24;
                if (i == buffer.Length - 1) {
                    data.Add(part);
                }
            }
            else if (i % 4 == 1) {
                part |= (uint)val << 16;
                if (i == buffer.Length - 1) {
                    data.Add(part);
                }
            }
            else if (i % 4 == 2) {
                part |= (uint)val << 8;
                if (i == buffer.Length - 1) {
                    data.Add(part);
                }
            }
            else {
                part |= (uint)val;
                data.Add(part);
                part = 0;
            }
        }

        return [.. data];
    }
}