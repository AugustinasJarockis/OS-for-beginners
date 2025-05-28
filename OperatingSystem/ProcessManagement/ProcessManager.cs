using OperatingSystem.Hardware.Constants;
using OperatingSystem.ProcessManagement.Processes;
using OperatingSystem.Utilities;
using Serilog;

namespace OperatingSystem.ProcessManagement;

public class ProcessManager
{
    private readonly ProcessPriorityQueue _processQueue;
    
    public Process CurrentProcess { get; private set; }
    public ushort CurrentProcessId => CurrentProcess.Id;

    public static ushort CLIProcessId { get; private set; }
    public List<Process> Processes { get; }

    public ProcessManager()
    {
        Processes = [];
        _processQueue = new();
    }
    
    public ushort CreateProcess(string processName, ProcessProgram processProgram, bool isCLI = false, bool isSystem = false)
    {
        if (Processes.Any(x => x.Name == processName))
        {
            throw new InvalidOperationException($"Process with such name already exists: {processName}");
        }
        
        var process = Process.Create(
            id: AllocateProcessId(),
            name: processName,
            program: processProgram,
            parent: CurrentProcess,
            isSystem: isSystem
        );

        if (isCLI) {
            CLIProcessId = process.Id;
            Console.WriteLine("CLI id: " + CLIProcessId);
        }

        CurrentProcess ??= process;

        if (CurrentProcess != process)
        {
            CurrentProcess.Children.Add(process);
        }

        Processes.Add(process);
        
        Log.Information("Created process {ProcessName} with pid {Pid}", processName, process.Id);
        
        return process.Id;
    }
    
    public List<ushort> KillProcess(string processName)
    {
        List<ushort> killedPids = [];
        KillProcessRecursively(processName, killedPids);
        return killedPids;
    }

    public void BlockProcess(ushort processId)
    {
        var process = Processes.FirstOrDefault(x => x.Id == processId);
        process?.Block();
    }
    
    public void UnblockProcess(ushort processId)
    {
        var process = Processes.FirstOrDefault(x => x.Id == processId);
        process?.Unblock();
    }
    
    public void SuspendProcess(ushort processId)
    {
        var process = Processes.FirstOrDefault(x => x.Id == processId);
        Log.Information("Pid {Pid} suspended", processId);
        process?.Suspend();
    }
    
    public void UnsuspendProcess(ushort processId)
    {
        var process = Processes.FirstOrDefault(x => x.Id == processId);
        Log.Information("Pid {Pid} unsuspended", processId);
        process?.Unsuspend();
    }

    public bool IsProcessSuspended(ushort processId)
    {
        var process = Processes.FirstOrDefault(x => x.Id == processId);
        return process?.State is ProcessState.ReadySuspended or ProcessState.BlockedSuspended;
    }
    
    public void Schedule()
    {
        while (true)
        {
            _processQueue.Enqueue(CurrentProcess);

            _processQueue.RemoveAllNotReady();
            foreach (var process in Processes) {
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

    private void KillProcessRecursively(string processName, List<ushort> killedPids)
    {
        var process = Processes.First(x => x.Name == processName);
        Log.Information("Killing process {ProcessName} with pid {Pid}", processName, process.Id);
        var childProcessNames = process.Children.Select(x => x.Name).ToList();
        foreach (var childProcessName in childProcessNames)
        {
            KillProcessRecursively(childProcessName, killedPids);
        }

        process.Parent?.Children.Remove(process);
        killedPids.Add(process.Id);
        _processQueue.Remove(process);
        Processes.Remove(process);
    }

    public Process FindProcessById(ushort pid)
    {
        return Processes.First(x => x.Id == pid);
    }

    public bool ProcessExists(ushort processId)
    {
        return Processes.Any(x => x.Id == processId);
    }
    
    public bool IsSystemProcess(ushort processId)
    {
        return Processes.FirstOrDefault(x => x.Id == processId)?.IsSystem ?? false;
    }
    
    private ushort AllocateProcessId()
    {
        var takenProcessIds = Processes.Select(x => x.Id).ToHashSet();
        
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