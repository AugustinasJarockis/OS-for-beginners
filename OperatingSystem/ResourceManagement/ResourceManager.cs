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

    public void DestroyAllResources() {
        _resources.Clear();
        Log.Information("All resources destroyed");
    }

    public void ReleaseProcessResources(ushort processId)
    {
        Log.Information("Releasing pid {Pid} resources", processId);
        
        foreach (var resource in _resources)
        {
            resource.Release(processId);
        }
    }
    
    public void SubscribeGrantedToPidChange<TPart>(string resourceName, Action<string, ushort?, ushort?> callback) where TPart : ResourcePart
    {
        var resource = (Resource<TPart>)_resources.First(x => x.Name == resourceName);
        resource.OnGrantedToPidChange.Add(callback);
    }

    public void AddResourcePart<TPart>(string resourceName, TPart part) where TPart : ResourcePart
    {
        var resource = (Resource<TPart>)_resources.First(x => x.Name == resourceName);
        resource.Parts.Add(part);

        Log.Debug("Adding resource {Resource} part {Part}", resourceName, part.Name);
        
        var pidsGrantedResource = resource.RunScheduler();
        foreach (var pid in pidsGrantedResource)
        {
            _processManager.UnblockProcess(pid);
        }
    }

    public void ReleaseResourcePart<TPart>(string resourceName, TPart part) where TPart : ResourcePart
    {
        Log.Debug("Releasing resource {Resource} part {Part}", resourceName, part.Name);

        var resource = (Resource<TPart>)_resources.First(x => x.Name == resourceName);
        resource.SetGrantedToPid(part, null, null);
        
        var pidsGrantedResource = resource.RunScheduler();
        foreach (var pid in pidsGrantedResource)
        {
            _processManager.UnblockProcess(pid);
        }
    }

    public void ChangeOwnership<TPart>(string resourceName, string partName, ushort newOwnerPid) where TPart : ResourcePart
    {
        var resource = (Resource<TPart>)_resources.First(x => x.Name == resourceName);
        var part = resource.Parts.First(p => p.Name == partName);
        var oldOwnerPid = part.GrantedToPid;
        var newOwner = _processManager.FindProcessById(newOwnerPid);
        resource.SetGrantedToPid(part, newOwner.Id, newOwner.Parent?.Id);
        
        _processManager.UnblockProcess(newOwnerPid);
        
        if (oldOwnerPid.HasValue)
        {
            _processManager.BlockProcess(oldOwnerPid.Value);
        }
    }

    public void RequestResource(string resourceName, string partName)
    {
        var resource = _resources.First(x => x.Name == resourceName);
        var processId = _processManager.CurrentProcessId;

        Log.Debug("Resource {Resource} part {Part} requested by {ProcessName}", resourceName, partName, _processManager.CurrentProcess.Name);
        
        resource.Requesters.Add(new ResourceRequester
        {
            ProcessId = processId,
            ProcessParentPid = _processManager.CurrentProcess.Parent?.Id,
            PartName = partName
        });

        var pidsGrantedResource = resource.RunScheduler();
        foreach (var pid in pidsGrantedResource)
        {
            _processManager.UnblockProcess(pid);
        }

        if (!pidsGrantedResource.Contains(processId))
        {
            _processManager.BlockProcess(processId);
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