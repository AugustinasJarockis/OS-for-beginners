using OperatingSystem.ResourceManagement.Resources;

namespace OperatingSystem.ResourceManagement.Schedulers;

public interface IResourceScheduler<TPart>
{
    void Run(Resource<TPart> resource);
}