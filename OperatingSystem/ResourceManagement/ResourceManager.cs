using OperatingSystem.ProcessManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;
using Serilog;

namespace OperatingSystem.ResourceManagement;

public class ResourceManager
{
    private readonly List<IResource> _resources;
    private readonly ProcessManager _processManager;
    
    public ResourceManager(ProcessManager processManager)
    {
        _processManager = processManager;
        _resources = [];
    }

    public void CreateResource<TPart>(
        string resourceName,
        List<TPart> parts,
        IResourceScheduler<TPart> scheduler) where TPart : ResourcePart
    {
        Log.Debug("Creating resource {ResourceName}", resourceName);
        var resource = Resource<TPart>.Create(
            name: resourceName,
            parts: parts,
            scheduler: scheduler
        );
        
        _resources.Add(resource);
    }

    public void AddResourcePart<TPart>(string resourceName, TPart part) where TPart : ResourcePart
    {
        var resource = (Resource<TPart>)_resources.First(x => x.Name == resourceName);
        resource.Parts.Add(part);

        Log.Debug("Adding resource {Resource} part {Part}", resourceName, part.Name);
        
        var pidsGrantedResource = resource.RunScheduler();
        foreach (var pid in pidsGrantedResource)
        {
            _processManager.ActivateProcess(pid);
        }
    }

    public void ReleaseResourcePart<TPart>(string resourceName, TPart part) where TPart : ResourcePart
    {
        Log.Debug("Releasing resource {Resource} part {Part}", resourceName, part.Name);

        var resource = (Resource<TPart>)_resources.First(x => x.Name == resourceName);
        part.GrantedToPid = null;
        
        var pidsGrantedResource = resource.RunScheduler();
        foreach (var pid in pidsGrantedResource)
        {
            _processManager.ActivateProcess(pid);
        }
    }

    public void RequestResource(string resourceName, string partName)
    {
        var resource = _resources.First(x => x.Name == resourceName);
        var currentProcessId = _processManager.CurrentProcessId;

        Log.Debug("Resource {Resource} part {Part} requested by {ProcessName}", resourceName, partName, _processManager.CurrentProcess.Name);
        
        resource.Requesters.Add(new ResourceRequester
        {
            ProcessId = currentProcessId,
            PartName = partName
        });

        var pidsGrantedResource = resource.RunScheduler();
        foreach (var pid in pidsGrantedResource)
        {
            _processManager.ActivateProcess(pid);
        }

        if (!pidsGrantedResource.Contains(currentProcessId))
        {
            _processManager.SuspendProcess(currentProcessId);
        }
    }

    public TPart ReadResource<TPart>(string resourceName, string partName) where TPart : ResourcePart
    {
        var resource = (Resource<TPart>)_resources.First(x => x.Name == resourceName);
        var part = resource.Parts.First(x => x.Name == partName);

        if (part.IsSingleUse)
        {
            resource.Parts.Remove(part);
        }
        
        return part;
    }
}