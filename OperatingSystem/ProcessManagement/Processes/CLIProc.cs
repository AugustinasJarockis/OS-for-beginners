using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using System.Diagnostics;

namespace OperatingSystem.ProcessManagement.Processes;

public class CLIProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;
    private readonly ProcessManager _processManager;
    
    public CLIProc(ResourceManager resourceManager, ProcessManager processManager)
    {
        _resourceManager = resourceManager;
        _processManager = processManager;
        _resourceManager.SubscribeGrantedToPidChange<FocusData>(ResourceNames.Focus, OnFocusedProcessChange);
    }
    private void OnFocusedProcessChange(string _, ushort? processId) => _focusedProcessId = processId;

    private ushort? _focusedProcessId;
    private List<string> _inputTokens;

    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.RequestResource(ResourceNames.Focus, nameof(FocusData));
                return CurrentStep + 1;
            }
            case 1:
            {
                _resourceManager.RequestResource(ResourceNames.UserInput, nameof(UserInputData));
                return CurrentStep + 1;
            }
            case 2:
            {
                var input = _resourceManager.ReadResource<UserInputData>(ResourceNames.UserInput, nameof(UserInputData)).Text;
                _inputTokens = input.Trim().ToLower().Split(' ').Select(token => token.Trim()).ToList();

                return _inputTokens[0] switch
                {
                    "start" => 5,
                    "process" => 6,
                    "focus" => 7,
                    "kill" => 8,
                    "suspend" => 9,
                    "unsuspend" => 10,
                    "shutdown" => 11,
                    "dir" => 12,
                    "create" => 13,
                    "delete" => 14,
                    "write" => 15,
                    "display" => 16,
                    _ => 17
                };
            }
            case 5: {
                    //TODO: Implement file start
                    return 1;
                }
            case 6:
            {
                foreach (var process in _processManager.Processes)
                {
                    PrintMessage($"Process: {process.Name}; PID: {process.Id}; State: {process.State}");
                }
                
                return 1;
            }
            case 7:
            {
                if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid))
                {
                    PrintMessage("Expected format: 'focus [PID]'");
                    return 1;
                }

                if (!_processManager.ProcessExists(pid))
                {
                    PrintMessage($"Process not found by pid {pid}");
                    return 1;
                }

                if (_processManager.IsSystemProcess(pid))
                {
                    PrintMessage("System process cannot be focused");
                    return 1;
                }
                
                _resourceManager.ChangeOwnership<FocusData>(ResourceNames.Focus, nameof(FocusData), pid);
                
                return 1;
            }
            case 8:
            {
                if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid))
                {
                    PrintMessage("Expected format: 'kill [PID]'");
                    return 1;
                }

                if (!_processManager.ProcessExists(pid))
                {
                    PrintMessage($"Process not found by pid {pid}");
                    return 1;
                }

                if (_processManager.IsSystemProcess(pid))
                {
                    PrintMessage("System process cannot be killed");
                    return 1;
                }

                var processName = _processManager.FindProcessById(pid).Parent!.Name;
                _resourceManager.AddResourcePart(ResourceNames.ProgramInMemory, new ProgramInMemoryData
                {
                    IsSingleUse = true,
                    IsEnd = true,
                    Name = nameof(ProgramInMemoryData),
                    JobGovernorId = processName
                });
                
                return 1;
            }
            case 9: {

                if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid))
                {
                    PrintMessage("Expected format: 'suspend [PID]'");
                    return 1;
                }
                

                if (!_processManager.ProcessExists(pid))
                {
                    PrintMessage($"Process not found by pid {pid}");

                    return 1;
                }


                if (_processManager.IsSystemProcess(pid))
                {
                    PrintMessage("System process cannot be suspended");
                    return 1;
                }

                var process = _processManager.FindProcessById(pid);

                if (process.State != ProcessState.Ready && process.State != ProcessState.Blocked)
                {
                    PrintMessage($"Process {pid} is not in a suspendable state (must be READY or BLOCKED)");
                    return 1;
                }

                _processManager.SuspendProcess(pid);

                return 1;
                }
            case 10: {

                if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid))
                {
                    PrintMessage("Expected format: 'unsuspend [PID]'");
                    return 1;
                }

                if (!_processManager.ProcessExists(pid))
                {
                    PrintMessage($"Process not found by pid {pid}");
                    return 1;
                }

                if (_processManager.IsSystemProcess(pid))
                {
                    PrintMessage("System process cannot be unsuspended");
                    return 1;
                }

                var process = _processManager.FindProcessById(pid);

                if (process.State != ProcessState.ReadySuspended && process.State != ProcessState.BlockedSuspended)
                {
                    PrintMessage($"Process {pid} is not in a suspended state (must be READYS or BLOCKEDS)");
                    return 1;
                }

                _processManager.ActivateProcess(pid);

                return 1;
                }
            case 11:
            {
                _resourceManager.AddResourcePart(ResourceNames.OsShutdown, new OsShutdownData {
                    Name = nameof(OsShutdownData),
                    IsSingleUse = true,
                });
                return 1;
            }
            case 12: {
                    // TODO: make dir
                    return 1;
                }
            case 13: {
                    // TODO: make create
                    return 1;
                }
            case 14: {
                    // TODO: make delete
                    return 1;
                }
            case 15: {
                    // TODO: make write
                    return 1;
                }
            case 16: {
                    // TODO: make display
                    return 1;
                }
            case 17:
            {
                PrintMessage("Unknown command");    
                return 1;
            }
            default:
                return 1;
        }
    }

    private void PrintMessage(string text)
    {
        _resourceManager.AddResourcePart(ResourceNames.TerminalOutput, new TerminalOutputData
        {
            Name = nameof(TerminalOutputData),
            IsSingleUse = true,
            ProcessId = _processManager.CurrentProcessId,
            Text = text,
        });
    }
}