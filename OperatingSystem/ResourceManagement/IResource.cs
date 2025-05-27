namespace OperatingSystem.ResourceManagement;

public interface IResource
{
    string Name { get; }
    List<ResourceRequester> Requesters { get; }
    List<ushort> RunScheduler();
    void Release(int processId);
}