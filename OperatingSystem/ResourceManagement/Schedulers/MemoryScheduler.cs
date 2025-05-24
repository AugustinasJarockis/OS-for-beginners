using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ProcessManagement;
using System.Collections.Generic;
using System.Linq;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class MemoryScheduler : ResourceSchedulerBase<MemoryBlock>
{
    private readonly ProcessManager _processManager;
    private readonly FileMemoryTable _fileMemoryTable;

    public MemoryScheduler(ProcessManager processManager, FileMemoryTable fileMemoryTable)
    {
        _processManager = processManager;
        _fileMemoryTable = fileMemoryTable;
    }

    public override List<ushort> Run(Resource<MemoryBlock> resource)
    {
        List<ResourceRequester> grantedRequesters = new();

        var sortedRequesters = resource.Requesters
            .OrderByDescending(r => _processManager.GetProcessById(r.ProcessId).Priority)
            .ToList();

        var fileUsedBlocks = _fileMemoryTable.GetUsedBlocks();

        foreach (var requester in sortedRequesters)
        {
            var freeBlock = resource.Parts.FirstOrDefault(block =>
                !block.IsAllocated && !block.Reserved && !fileUsedBlocks.Contains((ushort)block.BlockId));

            if (freeBlock == null)
                break; 

            freeBlock.Reserved = true;
            grantedRequesters.Add(requester);
        }

        foreach (var requester in grantedRequesters)
        {
            resource.Requesters.Remove(requester);
        }

        return grantedRequesters.Select(r => r.ProcessId).ToList();
    }
}