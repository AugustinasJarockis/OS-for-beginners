using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class TerminalOutputProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;

    public TerminalOutputProc(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.RequestResource(ResourceNames.TerminalOutput, nameof(TerminalOutputData));
                return CurrentStep + 1;
            }
            case 1:
            {
                var terminalOutputData = _resourceManager.ReadResource<TerminalOutputData>(ResourceNames.TerminalOutput, nameof(TerminalOutputData));
                
                // TODO: we should check here if the process has focus
                
                Console.WriteLine(terminalOutputData.Text);
                
                return 0;
            }
            default:
                return 0;
        }
    }
}