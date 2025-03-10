namespace MachineEmulator;

public class RAM
{
    private const long RamSize = 2 << 29;
    
    public RAM()
    {
        Data = new int[RamSize];
    }
    
    private int[] Data { get; }
    
    public byte GetByte(long address)
    {
        int dword = Data[address / 4];
        return (byte)((dword >> (int)((3 - address % 4) * 8)) & 0xFF);
    }

    public void SetByte(long address, byte value)
    {
        int dwordToSet = Data[address / 4];
        int shiftCount = (int)(3 - address % 4) * 8;
        int mask = 0xFF << shiftCount;
        Data[address / 4] = (int)((dwordToSet & (0xFFFFFFFF ^ mask)) + (value << shiftCount));
    }

    public int GetDWord(long address)
    {
        return Data[address / 4];
    }

    public void SetDWord(long address, int value)
    {
        Data[address / 4] = value;
    }
}