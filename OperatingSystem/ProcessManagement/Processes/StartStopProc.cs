using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

namespace OperatingSystem.ProcessManagement.Processes;

public class StartStopProc : ProcessProgram
{
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;

    public StartStopProc(ProcessManager processManager, ResourceManager resourceManager)
    {
        _processManager = processManager;
        _resourceManager = resourceManager;
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.CreateResource(ResourceNames.OsShutdown, [], new OsShutdownScheduler());
                _resourceManager.CreateResource(ResourceNames.ProgramInMemory, [
                    new ProgramInMemoryData
                    {
                        Name = nameof(ProgramInMemoryData),
                        MachineCode = "",
                        IsSingleUse = true
                    }
                ], new ProgramInMemoryScheduler());
                _resourceManager.CreateResource(ResourceNames.Interrupt, [], new InterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.JobGovernorInterrupt, [], new JobGovernorInterruptScheduler());
                
                _processManager.CreateProcess(nameof(MainProc), new MainProc(_processManager, _resourceManager));
                _processManager.CreateProcess(nameof(InterruptProc), new InterruptProc(_resourceManager));

                return CurrentStep + 1;
            }
            case 1:
            {
                _resourceManager.RequestResource(
                    ResourceNames.OsShutdown,
                    nameof(OsShutdownData)
                );

                return CurrentStep + 1;
            }
            case 2:
            {
                throw new NotImplementedException("OS shutdown not implemented");
            }
            default:
                return 0;
        }
    }
}