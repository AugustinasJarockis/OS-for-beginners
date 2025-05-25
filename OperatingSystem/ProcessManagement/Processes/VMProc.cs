using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class VMProc : ProcessProgram
{
    private readonly Guid _jobGovernorGuid;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly string _fromInterruptPartName;

    private bool _interruptOccurred;

    public VMProc(Guid jobGovernorGuid, ResourceManager resourceManager, Processor processor)
    {
        _jobGovernorGuid = jobGovernorGuid;
        _resourceManager = resourceManager;
        _processor = processor;
        _fromInterruptPartName = $"{nameof(FromInterruptData)}_{jobGovernorGuid}";
    }

    public void OnInterrupt(byte interruptCode)
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
        
        _resourceManager.RequestResource(ResourceNames.FromInterrupt, _fromInterruptPartName);
        _interruptOccurred = true;
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _processor.Step();
                return _interruptOccurred ? 1 : 0;
            }
            case 1:
            {
                _resourceManager.ReadResource<FromInterruptData>(ResourceNames.FromInterrupt, _fromInterruptPartName);
                _interruptOccurred = false;
                return 0;
            }
            default:
                return 0;
        }
    }
}