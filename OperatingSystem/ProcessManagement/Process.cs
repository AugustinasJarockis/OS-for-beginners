namespace OperatingSystem.ProcessManagement;

public class Process
{
    private static readonly TimeSpan PeriodicInterruptInterval = TimeSpan.FromMilliseconds(25);
    
    private DateTimeOffset _startedAt;
    
    public ushort Id { get; private set; }
    public string Name { get; private set; }
    public ProcessState State { get; private set; }
    public Process? Parent { get; private set; }
    public List<Process> Children { get; private set; }
    public byte BasePriority { get; private set; }
    public byte Priority { get; set; }
    public ProcessProgram Program { get; init; }
    
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
            Program = program,
            _startedAt = DateTimeOffset.MinValue
        };
    }

    public void Run() {
        State = ProcessState.Running;
        _startedAt = DateTimeOffset.Now;

        do
        {
            Program.Step();
        }
        while (State == ProcessState.Running && _startedAt.Add(PeriodicInterruptInterval) > DateTimeOffset.Now);
        
        if (State == ProcessState.Running)
        {
            // Periodic interrupt occurred
            State = ProcessState.Ready;
        }
    }
    
    public void Suspend()
    {
        State = State switch
        {
            ProcessState.Ready => ProcessState.ReadySuspended,
            ProcessState.Blocked => ProcessState.BlockedSuspended,
            ProcessState.Running => ProcessState.Blocked,
            _ => State
        };
    }

    public void Activate()
    {
        State = State switch
        {
            ProcessState.ReadySuspended => ProcessState.Ready,
            ProcessState.BlockedSuspended => ProcessState.Blocked,
            ProcessState.Blocked => ProcessState.Ready,
            _ => State
        };
    }
}