using System.Text;
using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.ProcessManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Files;

public class FileSystem
{
    private const uint BlockCount = SizeConstants.EXTERNAL_STORAGE_SIZE / 4;
    
    private readonly ExternalStorage _externalStorage;
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;
    private readonly ExternalStorageBlockMetadata[] _blocksMetadata;
    private readonly Dictionary<string, List<ExternalStorageBlockMetadata>> _blocksByFileName = new();

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

    public void WriteFile(FileHandleData fileHandle, string[] content)
    {
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
    }

    public string[] ReadFile(FileHandleData fileHandle)
    {
        
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