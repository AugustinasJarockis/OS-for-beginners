namespace OperatingSystem.ResourceManagement.ResourceParts;

public class TerminalOutputData : ResourcePart
{
    public required ushort ProcessId { get; set; }
    public required string Text { get; set; }
}