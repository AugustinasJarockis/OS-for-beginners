using OperatingSystem.Hardware.Constants;

namespace OperatingSystem.Hardware;

public class RAM : IDisposable
{
    private readonly string? _snapshotFilePath;   
    
    public RAM()
    {
        Data = new uint[SizeConstants.RAM_SIZE];
    }

    public RAM(string filePath, bool startClear = false)
    {
        Data = new uint[SizeConstants.RAM_SIZE];
        _snapshotFilePath = filePath;
        
        if (!startClear && File.Exists(filePath))
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Buffer.BlockCopy(fileData, 0, Data, 0, (int)SizeConstants.MB64);
            Buffer.BlockCopy(fileData, (int)SizeConstants.MB64, Data, 0x40000000, (int)SizeConstants.MB64);
        }
    }
    
    public void Dispose()
    {
        if (_snapshotFilePath is not null)
        {
            FileStream stream = File.Open(_snapshotFilePath, FileMode.Create);
            byte[] writeData = new byte[2 * SizeConstants.MB64];
            
            Buffer.BlockCopy(Data, 0, writeData, 0, (int)SizeConstants.MB64);
            stream.Write(writeData, 0, (int)SizeConstants.MB64);
            
            Buffer.BlockCopy(Data, 0x40000000, writeData, (int)SizeConstants.MB64, (int)SizeConstants.MB64);
            stream.Write(writeData, (int)SizeConstants.MB64, (int)SizeConstants.MB64);
            
            stream.Close();
        }
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