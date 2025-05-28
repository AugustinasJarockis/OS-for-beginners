using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class UserInputScheduler : ResourceSchedulerBase<UserInputData>
{
    private ushort? _focusedProcessId;
    private ushort? _focusedProcessParentPid;
    
    public UserInputScheduler(ResourceManager resourceManager)
    {
        resourceManager.SubscribeGrantedToPidChange<FocusData>(ResourceNames.Focus, OnFocusedProcessChange);
    }

    public override List<ushort> Run(Resource<UserInputData> resource)
    {
        if (resource.Parts.Count == 0)
        {
            return [];
        }
        
        var requester = resource.Requesters.FirstOrDefault(x => x.ProcessId == _focusedProcessId || x.ProcessId == _focusedProcessParentPid);
        if (requester is null)
        {
            return [];
        }

        return [requester.ProcessId];
    }

    private void OnFocusedProcessChange(string _, ushort? processId, ushort? parentProcessId)
    {
        _focusedProcessId = processId;
        _focusedProcessParentPid = parentProcessId;
    }
}
