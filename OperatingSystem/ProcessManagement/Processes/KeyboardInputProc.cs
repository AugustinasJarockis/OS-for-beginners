using System.Text;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class KeyboardInputProc : ProcessProgram
{
    private static readonly StringBuilder UserInputBuilder = new();
    
    private readonly ResourceManager _resourceManager;
    
    public KeyboardInputProc(ResourceManager resourceManager)
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
                
                if (input.PressedKey is '\n' or '\r')
                {
                    _resourceManager.AddResourcePart(ResourceNames.UserInput, new UserInputData
                    {
                        Name = nameof(UserInputData),
                        Text = UserInputBuilder.ToString(),
                        IsSingleUse = true
                    });
                    UserInputBuilder.Clear();
                }
                else
                {
                    UserInputBuilder.Append(input.PressedKey);
                }

                return 0;
            }
            default:
                return 0;
        }
    }
}