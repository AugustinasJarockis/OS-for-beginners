using OperatingSystem.Utilities;

namespace OperatingSystem.ProcessManagement;

public class ProcessManager
{
    private readonly List<Process> _processes;
    private readonly ProcessPriorityQueue _processQueue;
    
    private Process _currentProcess;

    public ushort CurrentProcessId => _currentProcess?.Id ?? 0;

    public ProcessManager()
    {
        _processes = [];
        _processQueue = new();
    }
    
    public void CreateProcess(string processName, ProcessProgram processProgram)
    {
        // TODO: add resources
        var process = Process.Create(
            id: AllocateProcessId(),
            name: processName,
            program: processProgram,
            parent: _currentProcess
        );

        _currentProcess = process;
        
        _processes.Add(process);
    }

    public void KillProcess(string processName)
    {
        KillProcessRecursively(processName);
        Schedule();
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

    private void KillProcessRecursively(string processName)
    {
        var process = FindProcessByName(processName);
        if (process is null)
        {
            return; // TODO: error?
        }
        
        // TODO: release resources
        
        var childProcessNames = process.Children.Select(x => x.Name).ToList();
        foreach (var childProcessName in childProcessNames)
        {
            KillProcessRecursively(childProcessName);
        }

        process.Parent?.Children.Remove(process);
        _processes.Remove(process);
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
        
        throw new InvalidOperationException("Could not allocate process id"); // TODO: maybe add some interrupt?
    }

    private Process? FindProcessByName(string processName)
    {
        return _processes.FirstOrDefault(x => x.Name == processName);
    }

    public void Schedule()
    {
        // TODO: this algorithm does not work - fix it
        // _processQueue.Enqueue(_currentProcess!);
        //
        // _processQueue.RemoveAllNotReady();
        // foreach (var process in _processes) {
        //     if (process.State == ProcessState.Ready 
        //         && !_processQueue.Contains(process)
        //         ) {
        //         _processQueue.Enqueue(process);
        //     }
        // }

        // _currentProcess = _processQueue.Dequeue();

        // _processQueue.IncrementPriorities();

        _currentProcess.Run();
    }
}