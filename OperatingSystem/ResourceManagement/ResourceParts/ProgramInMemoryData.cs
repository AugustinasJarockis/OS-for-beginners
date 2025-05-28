namespace OperatingSystem.ResourceManagement.ResourceParts;

public class ProgramInMemoryData : ResourcePart
{
    public string ProgramName { get; set; }
    public List<uint> MachineCode { get; set; }
    public bool IsEnd { get; set; }
    public string JobGovernorId { get; set; }
    public byte BasePriority { get; set; } = 0;
}