namespace OperatingSystem.ProcessManagement.Processes;

public class IdleProc : ProcessProgram
{
    protected override int Next()
    {
        return 0;
    }
}