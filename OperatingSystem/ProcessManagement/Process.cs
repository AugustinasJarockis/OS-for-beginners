namespace OperatingSystem.ProcessManagement;

public class Process
{
    public ushort Id { get; private set; }
    public string Name { get; private set; }
    public ProcessState State { get; private set; }
    public Process? Parent { get; private set; }
    public List<Process> Children { get; private set; }
    public byte BasePriority { get; private set; }
    public byte Priority { get; set; }
    public bool IsRunning => State == ProcessState.Running;

    public ProcessProgram Program { get; private set; }

    private Process()
    {
    }

    public static Process Create(ushort id, string name, ProcessProgram program, Process? parent)
    {
        return new Process
        {
            Id = id,
            Name = name,
            Children = [],
            Parent = parent,
            BasePriority = 0, // TODO: maybe set different base priority for different processes
            Priority = 0,
            State = ProcessState.Ready,
            Program = program
        };
    }

    public void Run() {
        State = ProcessState.Running;
        Program.Proceed();
        State = ProcessState.Ready;
    }
    
    public void Suspend()
    {
        State = State switch
        {
            ProcessState.Ready => ProcessState.ReadySuspended,
            ProcessState.Blocked => ProcessState.BlockedSuspended,
            _ => State
        };
    }

    public void Activate()
    {
        State = State switch
        {
            ProcessState.ReadySuspended => ProcessState.Ready,
            ProcessState.BlockedSuspended => ProcessState.Blocked,
            _ => State
        };
    }
}