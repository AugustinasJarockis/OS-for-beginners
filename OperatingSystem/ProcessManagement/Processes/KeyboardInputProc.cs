using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class KeyboardInputProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;

    public KeyboardInputProc(ResourceManager resourceManager) 
    {
        _resourceManager = resourceManager;
        _resourceManager.SubscribeGrantedToPidChange<FocusData>(ResourceNames.Focus, OnFocusedProcessChange);
    }

    private void OnFocusedProcessChange(string _, ushort? processId, ushort? _1) => _focusedProcessId = processId;

    private ushort? _focusedProcessId;
    private string currentInput = "";
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.RequestResource(ResourceNames.KeyboardInput, nameof(KeyboardInputData));
                return CurrentStep + 1;
            }
            case 1:
            {
                var input = _resourceManager.ReadResource<KeyboardInputData>(ResourceNames.KeyboardInput, nameof(KeyboardInputData));

                if (input.PressedKey == '\b') {
                    Console.Write(" \b");
                    if (currentInput.Length > 0)
                        currentInput = currentInput[..^1];
                }
                else if (input.PressedKey == '\r') {
                    Console.Write("\b\n");
                    currentInput += '\n';
                }
                else {
                        currentInput += input.PressedKey;
                }
                return 2;
            }
            case 2: 
            {
                if (currentInput.Length >= 1 && currentInput[^1] == '\n') {
                    return 3;
                }
                return 0;
            }
            case 3: 
            {
                if (ProcessManager.CLIProcessId == _focusedProcessId) {
                    return 4;
                }
                return 5;
            }
            case 4: 
            {
                _resourceManager.AddResourcePart(ResourceNames.UserInput, new UserInputData() {
                    Name = nameof(UserInputData),
                    Text = currentInput,
                    IsSingleUse = true
                });
                currentInput = "";
                return 0;
            }
            case 5: 
            {
                if (currentInput == ":wq\n") {
                    _resourceManager.ChangeOwnership<FocusData>(ResourceNames.Focus, nameof(FocusData), ProcessManager.CLIProcessId);
                    currentInput = "";
                    return 0;
                }

                _resourceManager.AddResourcePart(ResourceNames.UserInput, new UserInputData() {
                    Name = nameof(UserInputData),
                    Text = currentInput,
                    IsSingleUse = true
                });
                currentInput = "";
                return 0;
            }
            default:
                return 0;
        }
    }
}
