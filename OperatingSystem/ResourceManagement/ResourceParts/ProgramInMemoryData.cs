namespace OperatingSystem.ResourceManagement.ResourceParts;

public class ProgramInMemoryData : ResourcePart
{
    public List<uint> MachineCode { get; set; }
    public bool IsEnd { get; set; }
    public string JobGovernorId { get; set; }
}