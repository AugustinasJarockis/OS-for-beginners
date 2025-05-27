using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

namespace OperatingSystem.ResourceManagement;

public class Resource<TPart> : IResource where TPart : ResourcePart
{
    public string Name { get; private init; }
    public List<ResourceRequester> Requesters { get; private set; }
    public List<TPart> Parts { get; private init; }
    public IResourceScheduler<TPart> Scheduler { get; private init; }

    private Resource()
    {
    }

    public static Resource<TPart> Create(
        string name,
        List<TPart> parts,
        IResourceScheduler<TPart> scheduler)
    {
        return new Resource<TPart>
        {
            Name = name,
            Parts = parts,
            Scheduler = scheduler,
            Requesters = []
        };
    }

    public List<ushort> RunScheduler() => Scheduler.Run(this);
    
    public void Release(int processId)
    {
        Requesters = Requesters.Where(r => r.ProcessId != processId).ToList();
        var partsGrantedToProcess = Parts.Where(p => p.GrantedToPid == processId);
        foreach (var part in partsGrantedToProcess)
        {
            part.GrantedToPid = null;
        }
    }
}