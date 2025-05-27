using OperatingSystem.Hardware;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class MainProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;
    private readonly ProcessManager _processManager;
    private readonly Processor _processor;
    private readonly MemoryManager _memoryManager;

    private ProgramInMemoryData _programInMemoryData;

    public MainProc(ProcessManager processManager, ResourceManager resourceManager, Processor processor, MemoryManager memoryManager)
    {
        _resourceManager = resourceManager;
        _processor = processor;
        _memoryManager = memoryManager;
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
                    var jobGovernorPid = _processManager.FindProcessByName(_programInMemoryData.JobGovernorId).Id;
                    _processManager.KillProcess(_programInMemoryData.JobGovernorId);
                    _resourceManager.ReleaseProcessResources(jobGovernorPid);
                }
                else
                {
                    var pid = _processManager.CreateProcess(
                        $"{nameof(JobGovernorProc)}_{_programInMemoryData.ProgramName}",
                        new JobGovernorProc(
                            _programInMemoryData.ProgramName,
                            _programInMemoryData.MachineCode,
                            _processManager,
                            _resourceManager,
                            _processor,
                            _memoryManager
                        )
                    );
                    
                    _resourceManager.ChangeOwnership<FocusData>(ResourceNames.Focus, nameof(FocusData), pid);
                }

                return 0;
            }
            default:
                return 0;
        }
    }
}