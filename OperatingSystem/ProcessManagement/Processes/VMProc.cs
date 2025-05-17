using OperatingSystem.Hardware;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class VMProc : ProcessProgram
{
    private readonly Guid _jobGovernorGuid;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;

    public VMProc(Guid jobGovernorGuid, ResourceManager resourceManager, Processor processor)
    {
        _jobGovernorGuid = jobGovernorGuid;
        _resourceManager = resourceManager;
        _processor = processor;
    }

    public void HandleInterrupt(byte interruptCode)
    {
        _resourceManager.AddResourcePart(
            ResourceNames.Interrupt,
            new InterruptData
            {
                Name = nameof(InterruptData),
                IsSingleUse = true,
                JobGovernorGuid = _jobGovernorGuid,
                InterruptCode = interruptCode
            });
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _processor.Step();
                return 0;
            }
            default:
                return 0;
        }
    }
}