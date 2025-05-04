using OperatingSystem.ResourceManagement.Resources;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class MemoryScheduler : IResourceScheduler<MemoryPart>
{
    public void Run(Resource<MemoryPart> resource)
    {
        throw new NotImplementedException();
    }
}