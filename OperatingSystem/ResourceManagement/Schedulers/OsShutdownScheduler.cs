using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class OsShutdownScheduler : IResourceScheduler<OsShutdownData>
{
    public List<ushort> Run(Resource<OsShutdownData> resource)
    {
        return [];
    }
}