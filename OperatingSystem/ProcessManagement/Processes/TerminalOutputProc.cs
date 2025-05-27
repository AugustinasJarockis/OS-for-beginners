using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using Serilog;

namespace OperatingSystem.ProcessManagement.Processes;

public class TerminalOutputProc : ProcessProgram
{
    private static readonly Dictionary<ushort, List<string>> _bufferByPid = new();
    
    private readonly ResourceManager _resourceManager;
    
    private ushort? _focusedProcessId;

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
                    Log.Information("Pid {Pid} is not focused. Persisting terminal output {Text} to buffer.", terminalOutputData.ProcessId, terminalOutputData.Text);
                    
                    if (_bufferByPid.TryGetValue(terminalOutputData.ProcessId, out var buffer))
                    {
                        buffer.Add(terminalOutputData.Text);
                    }
                    else
                    {
                        _bufferByPid[terminalOutputData.ProcessId] = [terminalOutputData.Text];
                    }
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

    private void OnFocusedProcessChange(string _, ushort? processId)
    {
        _focusedProcessId = processId;
        if (processId.HasValue && _bufferByPid.TryGetValue(processId.Value, out var buffer))
        {
            Log.Information("Flushing terminal buffer for pid {Pid}", processId.Value);
            
            foreach (var str in buffer)
            {
                Console.WriteLine(str);
            }

            _bufferByPid.Remove(processId.Value);
        }
    }
}