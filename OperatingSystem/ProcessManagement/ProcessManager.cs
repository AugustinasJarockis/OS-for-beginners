using OperatingSystem.Hardware.Constants;
using OperatingSystem.ProcessManagement.Processes;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement;
using OperatingSystem.Utilities;
using Serilog;

namespace OperatingSystem.ProcessManagement;

public class ProcessManager
{
    private readonly List<Process> _processes;
    private readonly ProcessPriorityQueue _processQueue;
    
    public Process CurrentProcess { get; private set; }
    public ushort CurrentProcessId => CurrentProcess.Id;

    public static ushort CLIProcessId { get; private set; }

    public ProcessManager()
    {
        _processes = [];
        _processQueue = new();
    }
    
    public ushort CreateProcess(string processName, ProcessProgram processProgram, bool isCLI = false)
    {
        if (_processes.Any(x => x.Name == processName))
        {
            throw new InvalidOperationException($"Process with such name already exists: {processName}");
        }
        
        var process = Process.Create(
            id: AllocateProcessId(),
        name: processName,
        program: processProgram,
            parent: CurrentProcess
        );

        if (isCLI) {
            CLIProcessId = process.Id;
        }

        CurrentProcess ??= process;

        _processes.Add(process);
        
        Log.Information("Created process {ProcessName} with pid {Pid}", processName, process.Id);
        
        return process.Id;
    }
    
    public void KillProcess(string processName)
    {
        KillProcessRecursively(processName);
    }

    public void SuspendProcess(ushort processId)
    {
        var process = _processes.First(x => x.Id == processId);
        process.Suspend();
    }
    
    public void ActivateProcess(ushort processId)
    {
        var process = _processes.First(x => x.Id == processId);
        process.Activate();
    }
    
    public void Schedule()
    {
        while (true)
        {
            _processQueue.Enqueue(CurrentProcess);

            _processQueue.RemoveAllNotReady();
            foreach (var process in _processes) {
                if (process.State == ProcessState.Ready
                    && !_processQueue.Contains(process)
                   ) {
                    _processQueue.Enqueue(process);
                }
            }

            CurrentProcess = _processQueue.Dequeue();

            _processQueue.IncrementPriorities();

            CurrentProcess.Run();
        }
    }

    public void HandlePeriodicInterrupt()
    {
        if (CurrentProcess is null)
        {
            return;
        }
        
        if (CurrentProcess.Program is VMProc vmProc)
        {
            vmProc.OnInterrupt(InterruptCodes.PeriodicInterrupt);
        }
        else
        {
            CurrentProcess.State = ProcessState.Ready;
        }
    }

    private void KillProcessRecursively(string processName)
    {
        var process = FindProcessByName(processName);
        Log.Information("Killing process {ProcessName} with pid {Pid}", processName, process.Id);
        var childProcessNames = process.Children.Select(x => x.Name).ToList();
        foreach (var childProcessName in childProcessNames)
        {
            KillProcessRecursively(childProcessName);
        }

        process.Parent?.Children.Remove(process);
        _processes.Remove(process);
    }
    
    public Process FindProcessByName(string processName)
    {
        return _processes.First(x => x.Name == processName);
    }
    
    private ushort AllocateProcessId()
    {
        var takenProcessIds = _processes.Select(x => x.Id).ToHashSet();
        
        for (var i = ushort.MinValue; i < ushort.MaxValue; i++)
        {
            if (!takenProcessIds.Contains(i))
            {
                return i;
            }
        }
        
        throw new InvalidOperationException("Could not allocate process id");
    }
}