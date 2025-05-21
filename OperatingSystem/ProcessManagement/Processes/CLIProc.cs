using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class CLIProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;
    
    public CLIProc(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

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
                Console.WriteLine($"CLI received: {input.PressedKey}");
                return 0;
            }
            default:
                return 0;
        }
    }
}