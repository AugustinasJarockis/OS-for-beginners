namespace MachineEmulator;

public class RAM
{
    public const long RAM_SIZE = 2 << 29;
    public RAM()
    {
        Data = new int[RAM_SIZE];
    }

    public byte this[long key] {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    private byte GetValue(long key) {
        int dword = Data[key / 4];
        return (byte)((dword >> (int)((3 - key % 4) * 8)) & 0xFF);
    }

    private void SetValue(long key, byte value) {
        int dwordToSet = Data[key / 4];
        int shiftCount = ((int)(3 - key % 4) * 8);
        int mask = 0xFF << shiftCount;
        Data[key / 4] = (int)((dwordToSet & (0xFFFFFFFF ^ mask)) + (value << shiftCount));
    }

    private int[] Data { get; }
}