using System.Text;
using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.ProcessManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using Serilog;

namespace OperatingSystem.ResourceManagement.Files;

public class FileSystem
{
    private const uint BlockCount = SizeConstants.EXTERNAL_STORAGE_SIZE / 4;
    
    private readonly ExternalStorage _externalStorage;
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;
    private readonly ExternalStorageBlockMetadata[] _blocksMetadata;
    private readonly Dictionary<string, List<ExternalStorageBlockMetadata>> _blocksByFileName = new();

    private static readonly uint[] EmptyBlock = new uint[ExternalStorage.BLOCK_SIZE];

    public FileSystem(ExternalStorage externalStorage, ProcessManager processManager, ResourceManager resourceManager)
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
    
    public void CreateFile(string fileName)
    {
        if (_blocksByFileName.ContainsKey(fileName))
        {
            return;
        }

        _blocksByFileName.Add(fileName, []);
        _resourceManager.AddResourcePart(ResourceNames.FileHandle, new FileHandleData
        {
            Name = fileName,
            IsSingleUse = false,
        });
    }
    
    public void DeleteFile(FileHandleData fileHandle)
    {
        if (!_blocksByFileName.TryGetValue(fileHandle.Name, out var blocks))
        {
            return;
        }
        
        foreach (var block in blocks)
        {
            _externalStorage.WriteBlock(block.BlockIndex, EmptyBlock);
            block.AllocatedToPid = null;
        }

        _blocksByFileName.Remove(fileHandle.Name);
    }

    public void OverwriteFile(FileHandleData fileHandle, string[] content)
    {
        Log.Information("Writing to file {FileName}", fileHandle.Name);

        DeleteFile(fileHandle);
        
        // TODO: here should be a call to scheduler, but lets assume that we always have enough external storage :)
        
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

        var currentProcessId = _processManager.CurrentProcessId;
        for (var i = 0; i < blocksAllocatedToRequester.Count; i++)
        {
            var block = blocksAllocatedToRequester[i];
            block.AllocatedToPid = currentProcessId;
            var data = StringToUIntArray(blocksToWrite[i]);
            _externalStorage.WriteBlock(block.BlockIndex, data);
        }

        _blocksByFileName[fileHandle.Name] = blocksAllocatedToRequester;
    }

    public string[] ReadFile(FileHandleData fileHandle)
    {
        Log.Information("Reading from file {FileName}", fileHandle.Name);
        
        var blocksMetadata = _blocksByFileName[fileHandle.Name];
        List<string> content = [];

        foreach (var blockMetadata in blocksMetadata)
        {
            var block = _externalStorage.ReadBlock(blockMetadata.BlockIndex);
            var strBuilder = new StringBuilder();
            foreach (var part in block)
            {
                var ch1 = (char)((part & 0xFFFF0000) >> 16);
                var ch2 = (char)(part & 0x0000FFFF);

                if (ch1 == 0)
                    break;
                strBuilder.Append(ch1);

                if (ch2 == 0)
                    break;
                strBuilder.Append(ch2);
            }

            content.AddRange(strBuilder.ToString().Split(Environment.NewLine));
        }

        return content.ToArray();
    }

    private static string[] SplitToBlocks(string[] content)
    {
        var joinedContent = string.Join(Environment.NewLine, content);
        return joinedContent.Chunk(4096 / sizeof(char)).Select(c => new string(c)).ToArray();
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
}