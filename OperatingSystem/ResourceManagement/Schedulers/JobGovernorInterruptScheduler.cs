using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class JobGovernorInterruptScheduler : IResourceScheduler<JobGovernorInterruptData>
{
    public List<ushort> Run(Resource<JobGovernorInterruptData> resource)
    {
        throw new NotImplementedException();
    }
}