using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class MemoryScheduler : ResourceSchedulerBase<MemoryBlock>
{
    public override List<ushort> Run(Resource<MemoryBlock> resource)
    {
        throw new NotImplementedException("We might need a different implementation for memory. Override if needed.");
    }
}