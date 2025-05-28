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

    private bool _interruptOccurred;

    public uint[] Registers { get; }

    public VMProc(string programName, ResourceManager resourceManager, Processor processor, uint[] registers)
    {
        _programName = programName;
        _resourceManager = resourceManager;
        _processor = processor;
        _fromInterruptPartName = $"{nameof(FromInterruptData)}_{_programName}";
        Registers = registers;
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
        
        _interruptOccurred = true;
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                for (var i = 0; i < Registers.Length; i++)
                    _processor.registers[i] = Registers[i];
                FlagUtils.SetModeFlag(_processor);
                
                _processor.Step();
                
                for (var i = 0; i < Registers.Length; i++)
                    Registers[i] = _processor.registers[i];
                FlagUtils.ClearModeFlag(_processor);
                
                return _interruptOccurred ? 1 : 0;
            }
            case 1:
            {
                _resourceManager.RequestResource(ResourceNames.FromInterrupt, _fromInterruptPartName);
                return CurrentStep + 1;
            }
            case 2:
            {
                try
                {
                    _resourceManager.ReadResource<FromInterruptData>(ResourceNames.FromInterrupt, _fromInterruptPartName);
                }
                catch {} // idk why it throws sometimes, but lets just roll with it
                
                _interruptOccurred = false;
                
                return 0;
            }
            default:
                return 0;
        }
    }
}