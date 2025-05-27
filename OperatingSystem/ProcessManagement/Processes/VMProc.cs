using OperatingSystem.Hardware;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class VMProc : ProcessProgram
{
    private readonly string _programName;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly string _fromInterruptPartName;

    private bool _interruptOccurred;

    public VMProc(string programName, ResourceManager resourceManager, Processor processor)
    {
        _programName = programName;
        _resourceManager = resourceManager;
        _processor = processor;
        _fromInterruptPartName = $"{nameof(FromInterruptData)}_{_programName}";
    }

    public void OnInterrupt(byte interruptCode)
    {
        _resourceManager.AddResourcePart(
            ResourceNames.Interrupt,
            new InterruptData
            {
                Name = nameof(InterruptData),
                IsSingleUse = true,
                ProgramName = _programName,
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