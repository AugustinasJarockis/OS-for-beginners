using System.Text;
using OperatingSystem.Hardware;
using OperatingSystem.ProcessManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using Serilog;

namespace OperatingSystem.ResourceManagement;

public class MemoryManager
{
    public const int PageSize = 4096;
    
    private const int PageTablesBaseAddress = PageSize * 100;
    private const int PageTableSize = 4 * PageCount;
    private const int PageCount = 1048576; // 4GB / 4KB
    private const int KernelPageCount = 262144; // 4GB / 4KB / 4

    private readonly ProcessManager _processManager;
    private readonly RAM _ram;
    private readonly MemoryPageMetadata[] _pagesMetadata;
    private readonly List<MallocRequester> _mallocRequesters = [];
    private readonly Dictionary<ushort, List<MemoryPageMetadata>> _allocatedPagesByPid = new();
    
    public MemoryManager(ProcessManager processManager, RAM ram)
    {
        _processManager = processManager;
        _ram = ram;
        _pagesMetadata = new MemoryPageMetadata[PageCount];

        var currentPid = processManager.CurrentProcessId;
        var kernelPages = new List<MemoryPageMetadata>();
        _allocatedPagesByPid.Add(currentPid, kernelPages);
        
        for (var i = 0; i < PageCount; i++)
        {
            var isKernelSpace = i <= KernelPageCount;
            var metadata = new MemoryPageMetadata
            {
                PageIndex = i,
                AllocatedToPid = isKernelSpace ? currentPid : null,
            };

            _pagesMetadata[i] = metadata;
            if (isKernelSpace)
            {
                kernelPages.Add(metadata);
            }
        }
    }

    public void AllocateMemory(int bytesToAllocate)
    {
        var currentProcessId = _processManager.CurrentProcessId;
        var requester = new MallocRequester
        {
            ProcessId = currentProcessId,
            BytesToAllocate = bytesToAllocate,
        };
        
        _mallocRequesters.Add(requester);

        var pidsGrantedMemory = RunScheduler();
        foreach (var pid in pidsGrantedMemory)
        {
            _processManager.ActivateProcess(pid);
        }

        if (!pidsGrantedMemory.Contains(currentProcessId))
        {
            _processManager.SuspendProcess(currentProcessId);
        }
    }

    public void FreeMemory()
    {
        var currentProcessId = _processManager.CurrentProcessId;
        if (!_allocatedPagesByPid.TryGetValue(currentProcessId, out var pages))
        {
            return;
        }

        foreach (var page in pages)
        {
            page.AllocatedToPid = null;
        }
        
        Log.Information("Freed {PageCount} pages of memory from pid {Pid}", pages.Count, currentProcessId);

        _allocatedPagesByPid.Remove(currentProcessId);
    }

    public string GetStringUntilZero(ulong virtualAddress)
    {
        const int maxStrLength = 1024;
        var strBuilder = new StringBuilder();

        for (var i = 0; i < maxStrLength; i++)
        {
            var physicalAddress = CalculatePhysicalAddress(virtualAddress + (ulong)i, _processManager.CurrentProcessId);
            var ch = (char)_ram.GetByte(physicalAddress);

            if (ch == 0)
                break;
            
            strBuilder.Append(ch);
        }
        
        return strBuilder.ToString();
    }
    
    public void SetDWord(ulong virtualAddress, uint value)
    {
        var physicalAddress = CalculatePhysicalAddress(virtualAddress, _processManager.CurrentProcessId);
        _ram.SetDWord(physicalAddress, value);
    }
    
    public uint GetPageTableAddress(int processId)
    {
        return PageTablesBaseAddress + (uint)processId * PageTableSize;
    }
    
    private List<ushort> RunScheduler()
    {
        List<MallocRequester> requestersGrantedMemory = [];

        foreach (var requester in _mallocRequesters)
        {
            var neededPages = (int)Math.Ceiling((double)requester.BytesToAllocate / 4096);
            List<MemoryPageMetadata> pagesAllocatedToRequester = [];

            for (var i = 0; i < neededPages; i++)
            {
                var unallocatedPage = _pagesMetadata.FirstOrDefault(p => p.AllocatedToPid is null && !pagesAllocatedToRequester.Contains(p));
                if (unallocatedPage is not null)
                {
                    pagesAllocatedToRequester.Add(unallocatedPage);
                }
            }

            if (pagesAllocatedToRequester.Count != neededPages)
            {
                continue;
            }
            
            foreach (var page in pagesAllocatedToRequester)
            {
                page.AllocatedToPid = requester.ProcessId;
                requestersGrantedMemory.Add(requester);
            }
            
            if (_allocatedPagesByPid.TryGetValue(requester.ProcessId, out var previouslyAllocatedPages))
            {
                previouslyAllocatedPages.AddRange(pagesAllocatedToRequester);
            }
            else
            {
                _allocatedPagesByPid[requester.ProcessId] = pagesAllocatedToRequester;
            }
            
            Log.Information("Allocated {Pages} memory pages to pid {Pid}", pagesAllocatedToRequester.Count, requester.ProcessId);

            var processPages = _allocatedPagesByPid[requester.ProcessId];
            SetPageTableLength(requester.ProcessId, processPages.Count);
            for (var i = 0; i < processPages.Count; i++)
            {
                SetPageTableEntry(requester.ProcessId, processPages[i], i);
            }
        }

        foreach (var requester in requestersGrantedMemory)
        {
            _mallocRequesters.Remove(requester);
        }

        return requestersGrantedMemory.Select(x => x.ProcessId).ToList();
    }

    private void SetPageTableLength(int processId, int length)
    {
        var pageTableAddress = GetPageTableAddress(processId);
        _ram.SetDWord(pageTableAddress, (uint)length);
    }
    
    private void SetPageTableEntry(int processId, MemoryPageMetadata page, int virtualPageIndex)
    {
        var pageTableEntryAddress = GetPageTableAddress(processId) + (ulong)virtualPageIndex * 4 + 4;
        var pageTableEntry = (uint)(page.PageIndex << 1) + 1;
        _ram.SetDWord(pageTableEntryAddress, pageTableEntry);
    }

    private ulong CalculatePhysicalAddress(ulong virtualAddress, ushort processId)
    {
        var currentPid = _processManager.CurrentProcessId;
        var pages = _allocatedPagesByPid[currentPid];
        var pageIndex = (int)(virtualAddress / PageSize);
        var page = pages.ElementAt(pageIndex);
        var physicalAddress = (ulong)page.PageIndex * PageSize + virtualAddress % PageSize;
        return physicalAddress;
    }
}