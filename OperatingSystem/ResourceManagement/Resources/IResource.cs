namespace OperatingSystem.ResourceManagement.Resources;

public interface IResource
{
    string Name { get; }
    void RunScheduler();
}