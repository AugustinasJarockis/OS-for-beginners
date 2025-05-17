namespace OperatingSystem.ResourceManagement.ResourceParts;

public abstract class ResourcePart
{
    public string Name { get; set; }
    public bool IsSingleUse { get; set; }
    public ushort? GrantedToPid { get; set; } = null;
}