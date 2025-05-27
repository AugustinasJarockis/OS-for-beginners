using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class CLIProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;
    private readonly ProcessManager _processManager;
    
    public CLIProc(ResourceManager resourceManager, ProcessManager processManager)
    {
        _resourceManager = resourceManager;
        _processManager = processManager;
    }

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
                var userInput = _resourceManager.ReadResource<UserInputData>(ResourceNames.UserInput, nameof(UserInputData));
                
                // TODO: remove this, this is example how to use terminal
                // _resourceManager.AddResourcePart(ResourceNames.TerminalOutput, new TerminalOutputData
                // {
                //     Name = nameof(TerminalOutputData),
                //     IsSingleUse = true,
                //     ProcessId = _processManager.CurrentProcessId,
                //     Text = userInput.Text
                // });
                
                if (userInput.Text == "shutdown")
                {
                    _resourceManager.AddResourcePart(ResourceNames.OsShutdown, new OsShutdownData
                    {
                        Name = nameof(OsShutdownData),
                        IsSingleUse = true,
                    });
                }
                
                return 1;
            }
            default:
                return 0;
        }
    }
}