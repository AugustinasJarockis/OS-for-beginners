using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

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

                switch (_inputTokens[0])
                {
                    case "start":
                    {
                        return 5;
                    }
                    case "process":
                    {
                        return 6;
                    }
                    case "focus":
                    {
                        return 7;
                    }
                    case "kill":
                    {
                        return 8;
                    }
                    case "suspend":
                    {
                        return 9;
                    }
                    case "unsuspend":
                    {
                        return 10;
                    }
                    case "shutdown":
                    {
                        return 11;
                    }
                    case "dir":
                    {
                        return 12;
                    }
                    case "create":
                    {
                        return 13;
                    }
                    case "delete":
                    {
                        return 14;
                    }
                    case "write":
                    {
                        return 15;
                    }
                    case "display":
                    {
                        return 16;
                    }
                    default:
                    {
                        return 1;
                    }
                }
            }
            case 5: {
                    //TODO: Implement file start
                    return 1;
                }
            case 6: {
                    //TODO: Print all processes
                    return 1;
                }
            case 7: {
                    // TODO: error handling
                    // TODO: check if id exists
                    ushort pid;
                    ushort.TryParse(_inputTokens[1], out pid);
                    _resourceManager.ChangeOwnership<FocusData>(ResourceNames.Focus, nameof(FocusData), pid);
                    return 0;
                }
            case 8: {
                    // TODO: error handling
                    // TODO: check if id exists
                    ushort pid;
                    ushort.TryParse(_inputTokens[1], out pid);
                    // TODO: Kill process
                    return 1;
                }
            case 9: {
                    // TODO: make suspend
                    return 1;
                }
            case 10: {
                    // TODO: make unsuspend
                    return 1;
                }
            case 11: {
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
            case 17: {
                    // TODO: implement error handling
                    return 1;
                }
            default:
                return 1;
        }
    }
}