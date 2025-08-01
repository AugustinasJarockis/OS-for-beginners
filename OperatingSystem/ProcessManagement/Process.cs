namespace OperatingSystem.ProcessManagement;

public class Process
{
    public ushort Id { get; private set; }
    public string Name { get; private set; }
    public ProcessState State { get; set; }
    public Process? Parent { get; private set; }
    public List<Process> Children { get; private set; }
    public byte BasePriority { get; private set; }
    public byte Priority { get; set; }
    public ProcessProgram Program { get; init; }
    public bool IsSystem { get; set; }
    
    private Process()
    {
    }

    public static Process Create(ushort id, string name, ProcessProgram program, Process? parent, bool isSystem = false, byte basePriority = 0)
    {
        return new Process
        {
            Id = id,
            Name = name,
            Children = [],
            Parent = parent,
            BasePriority = basePriority,
            Priority = 0,
            State = ProcessState.Ready,
            Program = program,
            IsSystem = isSystem
        };
    }

    public void Run() {
        State = ProcessState.Running;
        do
        {
            Program.Step();
        }
        while (State == ProcessState.Running);
    }
    
    public void Block()
    {
        State = State switch
        {
            ProcessState.Ready or ProcessState.Running => ProcessState.Blocked,
            _ => State
        };
    }
    
    public void Unblock()
    {
        State = State switch
        {
            ProcessState.Blocked => ProcessState.Ready,
            ProcessState.BlockedSuspended => ProcessState.ReadySuspended,
            _ => State
        };
    }

    public void Suspend()
    {
        State = State switch
        {
            ProcessState.Ready => ProcessState.ReadySuspended,
            ProcessState.Blocked => ProcessState.BlockedSuspended,
            _ => State,
        };
    }
    
    public void Unsuspend()
    {
        State = State switch
        {
            ProcessState.ReadySuspended => ProcessState.Ready,
            ProcessState.BlockedSuspended => ProcessState.Blocked,
            _ => State,
        };
    }
}