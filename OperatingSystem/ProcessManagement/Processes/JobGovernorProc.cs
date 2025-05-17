using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

namespace OperatingSystem.ProcessManagement.Processes;

public class JobGovernorProc : ProcessProgram
{
    private readonly Guid _guid;
    private readonly string _machineCode;
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;

    public JobGovernorProc(
        Guid guid,
        string machineCode,
        ProcessManager processManager,
        ResourceManager resourceManager)
    {
        _guid = guid;
        _machineCode = machineCode;
        _processManager = processManager;
        _resourceManager = resourceManager;
    }

    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _processManager.CreateProcess(
                    $"{nameof(VMProc)}_{_guid}",
                    new VMProc(_guid, _machineCode, _resourceManager)
                );
                
                return CurrentStep + 1;
            }
            case 1:
            {
                _resourceManager.RequestResource(
                    ResourceNames.JobGovernorInterrupt,
                    $"{nameof(JobGovernorInterruptData)}_{_guid}"
                );

                return CurrentStep + 1;
            }
            default:
                return 0;
        }
    }
}