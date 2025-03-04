namespace MachineEmulator;

public class RAM
{
    public const long RAM_SIZE = 2 << 29;
    public RAM()
    {
        Data = new int[RAM_SIZE];
    }

    public byte this[int key] {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    private byte GetValue(int key) {
        int dword = Data[key / 4];
        return (byte)((dword >> ((3 - key % 4) * 8)) & 0xFF);
    }

    private void SetValue(int key, byte value) {
        int dwordToSet = Data[key / 4];
        int shiftCount = ((3 - key % 4) * 8);
        int mask = 0xFF << shiftCount;
        Data[key / 4] = (int)((dwordToSet & (0xFFFFFFFF ^ mask)) + (value << shiftCount));
    }

    private int[] Data { get; }
}