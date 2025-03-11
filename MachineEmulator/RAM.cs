namespace MachineEmulator;

public class RAM
{
    private const long RAM_SIZE = 2 << 29;
    
    public RAM()
    {
        Data = new uint[RAM_SIZE];
    }
    
    private uint[] Data { get; }
    
    public byte GetByte(ulong address)
    {
        uint dword = Data[address / 4];
        return (byte)((dword >> (int)((3 - address % 4) * 8)) & 0xFF);
    }

    public void SetByte(ulong address, byte value)
    {
        uint dwordToSet = Data[address / 4];
        int shiftCount = (int)(3 - address % 4) * 8;
        int mask = 0xFF << shiftCount;
        Data[address / 4] = (uint)((dwordToSet & (0xFFFFFFFF ^ mask)) + (value << shiftCount));
    }

    public uint GetDWord(ulong address)
    {
        return Data[address / 4];
    }

    public void SetDWord(ulong address, uint value)
    {
        Data[address / 4] = value;
    }
}