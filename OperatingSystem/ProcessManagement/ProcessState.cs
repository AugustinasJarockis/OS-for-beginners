namespace OperatingSystem.ProcessManagement;

public enum ProcessState
{
    Running = 1,
    Ready = 2,
    ReadySuspended = 3,
    Blocked = 4,
    BlockedSuspended = 5,
}