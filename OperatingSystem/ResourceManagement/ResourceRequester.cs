namespace OperatingSystem.ResourceManagement;

public class ResourceRequester
{
    public ushort ProcessId { get; set; }
    public required ushort? ProcessParentPid { get; set; }
    public string PartName { get; set; }
}