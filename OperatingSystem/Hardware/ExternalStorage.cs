using OperatingSystem.Hardware.Constants;

namespace OperatingSystem.Hardware;

public class ExternalStorage
{
    public const uint BLOCK_SIZE = 1024;

    private readonly uint[] _data;

    public ExternalStorage()
    {
        _data = new uint[SizeConstants.EXTERNAL_STORAGE_SIZE];
    }

    public void WriteBlock(uint blockIndex, uint[] data)
    {
        var baseAddress = blockIndex * BLOCK_SIZE;
        for (int i = 0; i < data.Length; i++)
        {
            _data[baseAddress + i] = data[i];
        }
    }

    public uint[] ReadBlock(uint blockIndex)
    {
        var data = new uint[BLOCK_SIZE];
        var baseAddress = blockIndex * BLOCK_SIZE;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = _data[baseAddress + i];
        }

        return data;
    }
}