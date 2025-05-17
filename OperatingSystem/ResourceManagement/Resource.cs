using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

namespace OperatingSystem.ResourceManagement;

public class Resource<TPart> : IResource where TPart : ResourcePart
{
    public string Name { get; private init; }
    public List<ResourceRequester> Requesters { get; private init; }
    public List<TPart> AvailableParts { get; private init; }
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
            AvailableParts = parts,
            Scheduler = scheduler,
            Requesters = []
        };
    }

    public List<ushort> RunScheduler() => Scheduler.Run(this);
}