namespace MachineEmulator;

public class RAM
{
    public RAM(byte[] data)
    {
        Data = data;
    }

    public byte[] Data { get; }
}