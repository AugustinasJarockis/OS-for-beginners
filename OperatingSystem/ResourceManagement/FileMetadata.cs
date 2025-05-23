using System.Collections.Generic;

namespace OperatingSystem.ResourceManagement;

public class FileMetadata
{
        public string FileName { get; set; }
        public List<ushort> MemoryBlocks { get; set; } = new List<ushort>();
        public bool IsSystemFile { get; set; }
}