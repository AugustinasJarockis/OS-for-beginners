using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public abstract class ResourceSchedulerBase<TPart> : IResourceScheduler<TPart> where TPart : ResourcePart
{
    public virtual List<ushort> Run(Resource<TPart> resource)
    {
        List<ResourceRequester> requestersGrantedResource = [];
        
        foreach (var requester in resource.Requesters)
        {
            var part = resource.Parts.FirstOrDefault(x => x.Name == requester.PartName && x.GrantedToPid is null);
            if (part is not null)
            {
                resource.SetGrantedToPid(part, requester.ProcessId);
                requestersGrantedResource.Add(requester);
            }
        }

        foreach (var requester in requestersGrantedResource)
        {
            resource.Requesters.Remove(requester);
        }
        
        return requestersGrantedResource.Select(x => x.ProcessId).ToList();
    }
}