namespace OperatingSystem.ResourceManagement.ResourceParts;

public class InterruptData : ResourcePart
{
    public string ProgramName { get; set; }
    public byte InterruptCode { get; set; }
}