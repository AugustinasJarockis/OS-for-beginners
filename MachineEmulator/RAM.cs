namespace MachineEmulator;

public class RAM
{
    private readonly byte[] _data;
    
    public RAM(byte[] data)
    {
        _data = data;
    }
}