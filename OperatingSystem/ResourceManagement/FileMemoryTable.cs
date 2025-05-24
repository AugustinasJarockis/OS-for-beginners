using System.Collections.Generic;

namespace OperatingSystem.ResourceManagement
{
    public class FileMemoryTable
    {
        public List<FileEntry> Files { get; } = new();
        public static int TotalBlocks { get; set; } = 16384;  // 64mb of 4 kb do we need that much?

        public List<ushort> GetUsedBlocks()
        {
            return Files.SelectMany(f => f.MemoryBlocks).Distinct().ToList();
        }

        public List<ushort> GetFreeBlocks()
        {
            var used = GetUsedBlocks();
            return Enumerable.Range(0, TotalBlocks)
                .Select(i => (ushort)i)
                .Where(i => !used.Contains(i))
                .ToList();
        }
    }

    public class FileEntry //Could be changed to make sure no other process can access it that knows it's file name
    {
        public string FileName { get; set; }
        public List<ushort> MemoryBlocks { get; set; } = new();
        public bool IsSystemFile { get; set; } = false ;
    }
}