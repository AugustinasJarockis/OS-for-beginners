using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class JobGovernorInterruptScheduler : IResourceScheduler<JobGovernorInterruptData>
{
    public List<ushort> Run(Resource<JobGovernorInterruptData> resource)
    {
        List<ushort> pidsGrantedResource = [];
        var availablePartNames = resource.AvailableParts.Select(x => x.Name).ToList();
        
        foreach (var requester in resource.Requesters)
        {
            if (availablePartNames.Contains(requester.PartName))
            {
                pidsGrantedResource.Add(requester.ProcessId);
                availablePartNames.Remove(requester.PartName);
            }
        }

        return pidsGrantedResource;
    }
}