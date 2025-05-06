using OperatingSystem.ResourceManagement;
using OperatingSystem.Utilities;

namespace OperatingSystem.ProcessManagement;

public class ProcessManager
{
    private readonly ResourceManager _resourceManager;
    private readonly List<Process> _processes;
    private readonly ProcessPriorityQueue _processQueue;
    
    private Process? _currentProcess;

    public ProcessManager(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
        _processes = [];
        _processQueue = new();
        _currentProcess = null;
    }
    
    public void CreateProcess(string processName)
    {
        // TODO: add resources
        var process = Process.Create(
            id: AllocateProcessId(),
            name: processName,
            parent: _currentProcess
        );
        
        _processes.Add(process);
    }

    public void KillProcess(string processName)
    {
        KillProcessRecursively(processName);
        Schedule();
    }

    public void SuspendProcess(string processName)
    {
        var process = FindProcessByName(processName);
        if (process is null)
        {
            return; // TODO: error?
        }

        if (process.IsRunning)
        {
            ReleaseProcessor(process);
        }
        
        process.Suspend();
        Schedule();
    }
    
    public void ActivateProcess(string processName)
    {
        var process = FindProcessByName(processName);
        if (process is null)
        {
            return; // TODO: error?
        }

        process.Activate();
        Schedule();
    }

    private void KillProcessRecursively(string processName)
    {
        var process = FindProcessByName(processName);
        if (process is null)
        {
            return; // TODO: error?
        }

        if (process.IsRunning)
        {
            ReleaseProcessor(process);
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

    private void ReleaseProcessor(Process process)
    {
        _resourceManager.ReleaseResource(ResourceNames.Processor);
    }

    private void Schedule()
    {
        _processQueue.Enqueue(_currentProcess!);
        
        _processQueue.RemoveAllNotReady();
        foreach (var process in _processes) {
            if (process.State == ProcessState.Ready 
                && !_processQueue.Contains(process)
                ) {
                _processQueue.Enqueue(process);
            }
        }

        _currentProcess = _processQueue.Deenqueue();

        _processQueue.IncrementPriorities();

        //TODO: Run current process somehow
    }
}