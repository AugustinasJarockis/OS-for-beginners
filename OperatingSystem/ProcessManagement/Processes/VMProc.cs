using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class VMProc : ProcessProgram
{
    private readonly Guid _jobGovernorGuid;
    private readonly string _machineCode;
    private readonly ResourceManager _resourceManager;

    public VMProc(Guid jobGovernorGuid, string machineCode, ResourceManager resourceManager)
    {
        _jobGovernorGuid = jobGovernorGuid;
        _machineCode = machineCode;
        _resourceManager = resourceManager;
    }
    
    protected override int Next()
    {
        // TODO: this is mock - use hardware here
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.AddResourcePart(
                    ResourceNames.Interrupt,
                    new InterruptData
                    {
                        Name = nameof(InterruptData),
                        JobGovernorGuid = _jobGovernorGuid,
                        InterruptCode = 4
                    }
                );

                return 0;
            }
            default:
                return 0;
        }
    }
}