namespace OperatingSystem.ResourceManagement.Files;

public class ExternalStorageBlockMetadata
{
    public uint BlockIndex { get; set; }
    public ushort? AllocatedToPid { get; set; } = null;
}