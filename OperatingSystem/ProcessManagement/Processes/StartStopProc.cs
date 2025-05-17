using OperatingSystem.Hardware;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

namespace OperatingSystem.ProcessManagement.Processes;

public class StartStopProc : ProcessProgram
{
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly RAM _ram;

    public StartStopProc(
        ProcessManager processManager,
        ResourceManager resourceManager,
        Processor processor,
        RAM ram)
    {
        _processManager = processManager;
        _resourceManager = resourceManager;
        _processor = processor;
        _ram = ram;
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.CreateResource(ResourceNames.OsShutdown, [], new OsShutdownScheduler());
                _resourceManager.CreateResource(ResourceNames.Interrupt, [], new InterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.JobGovernorInterrupt, [], new JobGovernorInterruptScheduler());
                
                _processManager.CreateProcess(nameof(MainProc), new MainProc(_processManager, _resourceManager, _processor, _ram));
                _processManager.CreateProcess(nameof(InterruptProc), new InterruptProc(_resourceManager));
                _processManager.CreateProcess(nameof(IdleProc), new IdleProc());

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