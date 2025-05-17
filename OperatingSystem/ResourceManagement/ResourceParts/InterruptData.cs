namespace OperatingSystem.ResourceManagement.ResourceParts;

public class InterruptData : ResourcePart
{
    public Guid JobGovernorGuid { get; set; }
    public int InterruptCode { get; set; }
}