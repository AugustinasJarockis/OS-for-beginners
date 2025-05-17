namespace OperatingSystem.ProcessManagement;

public abstract class ProcessProgram
{
    protected int CurrentStep = 0;

    public void Step() => CurrentStep = Next();
    
    protected abstract int Next();
}