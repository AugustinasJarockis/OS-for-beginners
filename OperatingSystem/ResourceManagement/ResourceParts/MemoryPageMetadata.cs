namespace OperatingSystem.ResourceManagement.ResourceParts;

public class MemoryPageMetadata
{
    public int PageIndex { get; set; }
    public ushort? AllocatedToPid { get; set; } = null;
}