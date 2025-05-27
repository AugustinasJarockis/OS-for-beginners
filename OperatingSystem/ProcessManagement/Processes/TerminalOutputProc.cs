using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class TerminalOutputProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;
    
    private ushort? _focusedProcessId = null;

    public TerminalOutputProc(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
        _resourceManager.SubscribeGrantedToPidChange<FocusData>(ResourceNames.Focus, OnFocusedProcessChange);
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
                
                if (_focusedProcessId != terminalOutputData.ProcessId)
                {
                    // TODO: store in buffer
                }
                else
                {
                    Console.WriteLine($"From terminal: {terminalOutputData.Text}");
                }
                
                return 0;
            }
            default:
                return 0;
        }
    }
    
    private void OnFocusedProcessChange(string _, ushort? processId) => _focusedProcessId = processId;
}