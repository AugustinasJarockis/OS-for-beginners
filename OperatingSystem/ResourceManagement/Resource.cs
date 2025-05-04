namespace OperatingSystem.ResourceManagement;

public class Resource
{
    public string Name { get; private set; }

    private Resource()
    {
    }

    public static Resource Create(string name)
    {
        return new Resource
        {
            Name = name
        };
    }
}