namespace OperatingSystem.ResourceManagement;

public class MallocRequester
{
    public ushort ProcessId { get; set; }
    public int BytesToAllocate { get; set; }
}