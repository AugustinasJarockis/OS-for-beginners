using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Operations;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class VMProc : ProcessProgram
{
    private readonly string _programName;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly string _fromInterruptPartName;
    private readonly uint[] _registers;

    private bool _interruptOccurred;

    public VMProc(string programName, ResourceManager resourceManager, Processor processor, uint[] registers)
    {
        _programName = programName;
        _resourceManager = resourceManager;
        _processor = processor;
        _fromInterruptPartName = $"{nameof(FromInterruptData)}_{_programName}";
        _registers = registers;
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
                for (var i = 0; i < _registers.Length; i++)
                    _processor.registers[i] = _registers[i];
                FlagUtils.SetModeFlag(_processor);
                
                _processor.Step();
                
                for (var i = 0; i < _registers.Length; i++)
                    _registers[i] = _processor.registers[i];
                FlagUtils.ClearModeFlag(_processor);
                
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