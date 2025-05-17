using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class ProgramInMemoryScheduler : IResourceScheduler<ProgramInMemoryData>
{
    public List<ushort> Run(Resource<ProgramInMemoryData> resource)
    {
        return [resource.Requesters.First().ProcessId];
    }
}