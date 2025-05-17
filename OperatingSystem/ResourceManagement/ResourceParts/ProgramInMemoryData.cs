namespace OperatingSystem.ResourceManagement.ResourceParts;

public class ProgramInMemoryData : ResourcePart
{
    public string MachineCode { get; set; }
    public bool IsEnd { get; set; }
    public string JobGovernorId { get; set; }
}