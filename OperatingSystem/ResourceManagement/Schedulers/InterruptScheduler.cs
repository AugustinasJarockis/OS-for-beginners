using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class InterruptScheduler : IResourceScheduler<InterruptData>
{
    public List<ushort> Run(Resource<InterruptData> resource)
    {
        return [];
    }
}