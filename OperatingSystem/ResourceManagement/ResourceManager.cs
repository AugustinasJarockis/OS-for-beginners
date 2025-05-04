namespace OperatingSystem.ResourceManagement;

public class ResourceManager
{
    private readonly List<Resource> _resources;

    public ResourceManager()
    {
        _resources = [];
    }

    public void ReleaseResource(string resourceName)
    {
        // TODO: implement
    }

    private Resource? FindResourceByName(string resourceName)
    {
        return _resources.FirstOrDefault(x => x.Name == resourceName);
    }
}