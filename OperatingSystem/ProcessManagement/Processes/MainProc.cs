using OperatingSystem.Hardware;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class MainProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;
    private readonly ProcessManager _processManager;
    private readonly Processor _processor;
    private readonly RAM _ram;

    private ProgramInMemoryData _programInMemoryData;

    public MainProc(ProcessManager processManager, ResourceManager resourceManager, Processor processor, RAM ram)
    {
        _resourceManager = resourceManager;
        _processor = processor;
        _ram = ram;
        _processManager = processManager;
    }

    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.RequestResource(ResourceNames.ProgramInMemory, nameof(ProgramInMemoryData));
                return CurrentStep + 1;
            }
            case 1:
            {
                _programInMemoryData = _resourceManager.ReadResource<ProgramInMemoryData>(
                    ResourceNames.ProgramInMemory, nameof(ProgramInMemoryData));
                return CurrentStep + 1;   
            }
            case 2:
            {
                if (_programInMemoryData.IsEnd)
                {
                    _processManager.KillProcess(_programInMemoryData.JobGovernorId);
                }
                else
                {
                    var guid = Guid.NewGuid();
                    _processManager.CreateProcess(
                        $"{nameof(JobGovernorProc)}_{guid}",
                        new JobGovernorProc(
                            guid,
                            _programInMemoryData.MachineCode,
                            _processManager,
                            _resourceManager,
                            _processor,
                            _ram
                        )
                    );
                }

                return 0;
            }
            default:
                return 0;
        }
    }
}