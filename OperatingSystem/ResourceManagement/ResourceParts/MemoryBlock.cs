namespace OperatingSystem.ResourceManagement.ResourceParts;

public class MemoryBlock : ResourcePart
{
    public int BlockId { get; set; }
    public int Size { get; set; } = 4096; //4 KB
    public bool IsAllocated { get; set; }
    public bool Reserved { get; set; } = false;
    public byte[] Data { get; private set; }

    public MemoryBlock(int blockId)
    {
        BlockId = blockId;
        Data = new byte[Size];
        IsAllocated = false;
        Reserved = false;
    }

    public void WriteData(byte[] inputData)
    {
        if (inputData.Length > Size)
            throw new ArgumentException("Input data exceeds block size."); // TO DO implement interrupt

        Array.Copy(inputData, Data, inputData.Length);
        IsAllocated = true;
    }

    public byte[] ReadData()
    {
        if (!IsAllocated)
            throw new InvalidOperationException("Memory block is not allocated."); // TO DO implement interrupt

        return Data;
    }

    public void Clear()
    {
        Array.Clear(Data, 0, Data.Length);
        IsAllocated = false;
        Reserved = false;
    }
}