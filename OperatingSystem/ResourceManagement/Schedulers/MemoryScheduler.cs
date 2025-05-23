using OperatingSystem.ResourceManagement.ResourceParts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OperatingSystem.ResourceManagement.Schedulers;

public class MemoryScheduler : ResourceSchedulerBase<MemoryBlock>
{
    private const int BlockSize = 4096;
    private const int TotalBlocks = 16384;  // 64mb of 4 kb do we need that much? 

    private readonly MemoryBlock[] _memoryBlocks = new MemoryBlock[TotalBlocks];
    private readonly Dictionary<string, FileMetadata> _fileRegistry = new();

    public MemoryScheduler()
    {
        for (int i = 0; i < TotalBlocks; i++)
        {
            _memoryBlocks[i] = new MemoryBlock(i);
        }
    }

    public override List<ushort> Run(Resource<MemoryBlock> resource)
    {
        var allocatedBlockIds = new List<ushort>();

        try
        {
            foreach (var _ in resource.Parts)
            {
                var freeBlock = _memoryBlocks.FirstOrDefault(b => !b.IsAllocated);
                if (freeBlock == null)
                    throw new OutOfMemoryException("No free memory blocks available."); //TO DO exception handling

                freeBlock.IsAllocated = true;
                allocatedBlockIds.Add((ushort)freeBlock.BlockId);
            }
        }
        catch
        {
            foreach (var blockId in allocatedBlockIds)
            {
                _memoryBlocks[blockId].Clear();
            }
            throw;
        }

        return allocatedBlockIds;
    }

    public void AddFile(string filePath, bool isSystemFile)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File does not exist."); //TO DO exception handling

        string fileName = Path.GetFileName(filePath);

        if (_fileRegistry.ContainsKey(fileName))
            throw new InvalidOperationException("A file with this name already exists in memory."); // TO DO exception handling

        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
            throw new ArgumentException("File name too long.");

        if (!Regex.IsMatch(fileName, @"^[A-Za-z0-9()._\-]+$"))
            throw new ArgumentException("File name contains illegal characters."); // TO DO exception handling

        long fileSize = new FileInfo(filePath).Length;
        int blocksNeeded = (int)Math.Ceiling(fileSize / (double)BlockSize);

        var resource = new Resource<MemoryBlock>();
        for (int i = 0; i < blocksNeeded; i++)
        {
            resource.Parts.Add(new MemoryBlock(-1));
        }

        var allocatedBlockIds = Run(resource);

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        byte[] buffer = new byte[BlockSize];
        int blockIndex = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, BlockSize)) > 0)
        {
            var blockId = allocatedBlockIds[blockIndex];
            var block = _memoryBlocks[blockId];
            block.WriteData(buffer[..bytesRead]);
            blockIndex++;
        }

        _fileRegistry[fileName] = new FileMetadata
        {
            FileName = fileName,
            FileSize = fileSize,
            MemoryBlocks = allocatedBlockIds,
            IsSystemFile = isSystemFile
        };
    }

    public void RemoveFile(string fileName)
    {
        if (!_fileRegistry.TryGetValue(fileName, out var file))
            throw new KeyNotFoundException("File not found."); //TO DO exception handling

        foreach (var blockId in file.MemoryBlocks)
        {
            _memoryBlocks[blockId].Clear();
        }

        _fileRegistry.Remove(fileName);
    }

    public byte[] ReadFile(string fileName)
    {
        if (!_fileRegistry.TryGetValue(fileName, out var file))
            throw new KeyNotFoundException("File not found."); //TO DO exception handling

        var fileData = new List<byte>();
        long bytesRemaining = file.FileSize;

        foreach (var blockId in file.MemoryBlocks)
        {
            var block = _memoryBlocks[blockId];
            var blockData = block.ReadData();

            int bytesToTake = (int)Math.Min(blockData.Length, bytesRemaining);
            fileData.AddRange(blockData.Take(bytesToTake));

            bytesRemaining -= bytesToTake;
            if (bytesRemaining <= 0) break;
        }

        return fileData.ToArray();
    }

    public IEnumerable<FileMetadata> ListFiles()
    {
        return _fileRegistry.Values;
    }
}
