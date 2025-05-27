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
                _resourceManager.RequestResource(ResourceNames.KeyboardInput, nameof(KeyboardInputData));
                return CurrentStep + 1;
            }
            case 2:
            {
                var keyboardInput = _resourceManager.ReadResource<KeyboardInputData>(ResourceNames.KeyboardInput, nameof(KeyboardInputData));
                _resourceManager.AddResourcePart(ResourceNames.TerminalOutput, new TerminalOutputData
                {
                    Name = nameof(TerminalOutputData),
                    IsSingleUse = true,
                    ProcessId = _processManager.CurrentProcessId,
                    Text = keyboardInput.PressedKey.ToString()
                });
                return 1;
            }
            default:
                return 0;
        }
    }
}