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
    private ushort? _focusedProcessId;

    public MainProc(ProcessManager processManager, ResourceManager resourceManager, Processor processor, MemoryManager memoryManager)
    {
        _resourceManager = resourceManager;
        _processor = processor;
        _memoryManager = memoryManager;
        _processManager = processManager;
        _resourceManager.SubscribeGrantedToPidChange<FocusData>(ResourceNames.Focus, OnFocusedProcessChange);
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
                    var vmProc = _processManager.Processes.First(x => x.Name == _programInMemoryData.JobGovernorId).Children.First();
                    if (vmProc.Id == _focusedProcessId)
                    {
                        _resourceManager.ChangeOwnership<FocusData>(ResourceNames.Focus, nameof(FocusData), ProcessManager.CLIProcessId);
                    }
                    
                    var killedPids = _processManager.KillProcess(_programInMemoryData.JobGovernorId);
                    foreach (var killedPid in killedPids)
                    {
                        _resourceManager.ReleaseProcessResources(killedPid);
                        _memoryManager.FreeMemory(killedPid);
                    }
                }
                else
                {
                    _processManager.CreateProcess(
                        $"{nameof(JobGovernorProc)}_{_programInMemoryData.ProgramName}",
                        new JobGovernorProc(
                            _programInMemoryData.ProgramName,
                            _programInMemoryData.MachineCode,
                            _processManager,
                            _resourceManager,
                            _processor,
                            _memoryManager
                        ),
                        isSystem: true
                    );
                }

                return 0;
            }
            default:
                return 0;
        }
    }
    
    private void OnFocusedProcessChange(string _, ushort? processId, ushort? _1) => _focusedProcessId = processId;
}