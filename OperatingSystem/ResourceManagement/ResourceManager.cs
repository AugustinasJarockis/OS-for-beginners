using OperatingSystem.ResourceManagement.Resources;
using OperatingSystem.ResourceManagement.Schedulers;

namespace OperatingSystem.ResourceManagement;

public class ResourceManager
{
    private readonly List<IResource> _resources;

    public ResourceManager()
    {
        _resources = [];
    }

    public void CreateResource<TPart>(
        string resourceName,
        List<TPart> parts,
        IResourceScheduler<TPart> scheduler)
    {
        var resource = Resource<TPart>.Create(
            name: resourceName,
            parts: parts,
            scheduler: scheduler
        );
        
        _resources.Add(resource);
    }

    public void ReleaseResource(string resourceName)
    {
        // TODO: implement
    }

    private IResource? FindResourceByName(string resourceName)
    {
        return _resources.FirstOrDefault(x => x.Name == resourceName);
    }
}