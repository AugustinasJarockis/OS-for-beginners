using Assembler;
using OperatingSystem.Hardware;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.Files;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

namespace OperatingSystem.ProcessManagement.Processes;

public class StartStopProc : ProcessProgram
{
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly RAM _ram;
    private readonly ExternalStorage _externalStorage;

    private MemoryManager _memoryManager;
    private FileSystem _fileSystem;

    public StartStopProc(
        ProcessManager processManager,
        ResourceManager resourceManager,
        Processor processor,
        RAM ram,
        ExternalStorage externalStorage)
    {
        _processManager = processManager;
        _resourceManager = resourceManager;
        _processor = processor;
        _ram = ram;
        _externalStorage = externalStorage;
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _memoryManager = new MemoryManager(_processManager, _ram);
                _fileSystem = new FileSystem(_externalStorage, _processManager, _resourceManager);
                
                _resourceManager.CreateResource(ResourceNames.OsShutdown, [], new OsShutdownScheduler());
                _resourceManager.CreateResource(ResourceNames.Interrupt, [], new InterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.JobGovernorInterrupt, [], new JobGovernorInterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.KeyboardInput, [], new KeyboardInputScheduler());
                _resourceManager.CreateResource(ResourceNames.NonExistent, [], new NonExistentResourceScheduler());
                _resourceManager.CreateResource(ResourceNames.FromInterrupt, [], new FromInterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.ProgramInMemory, [], new ProgramInMemoryScheduler());
                _resourceManager.CreateResource(ResourceNames.FileOperation, [], new FileOperationScheduler());
                _resourceManager.CreateResource(ResourceNames.FileHandle, [], new FileHandleScheduler());
                
                _processManager.CreateProcess(nameof(MainProc), new MainProc(_processManager, _resourceManager, _processor, _memoryManager));
                _processManager.CreateProcess(nameof(InterruptProc), new InterruptProc(_resourceManager));
                _processManager.CreateProcess(nameof(IdleProc), new IdleProc());
                _processManager.CreateProcess(nameof(CLIProc), new CLIProc(_resourceManager));
                _processManager.CreateProcess(nameof(FileManagerProc), new FileManagerProc(_resourceManager));

                TransferDataFileToExternalStorage("test.txt");
                LoadProgram("test.txt");

                return CurrentStep + 1;
            }
            case 1:
            {
                _resourceManager.RequestResource(
                    ResourceNames.OsShutdown,
                    nameof(OsShutdownData)
                );

                return CurrentStep + 1;
            }
            case 2:
            {
                throw new NotImplementedException("OS shutdown not implemented");
            }
            default:
                return 0;
        }
    }

    private void TransferDataFileToExternalStorage(string fileName)
    {
        _fileSystem.CreateFile(fileName);
        _resourceManager.RequestResource(ResourceNames.FileHandle, fileName);
        var fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, fileName);
        
        var dataFilePath = Path.Join(Environment.CurrentDirectory, "Data", fileName);
        var lines = File.ReadAllLines(dataFilePath);
        
        _fileSystem.OverwriteFile(fileHandle, lines);
        _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
    }

    private void LoadProgram(string fileName)
    {
        _resourceManager.RequestResource(ResourceNames.FileHandle, fileName);
        var fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, fileName);
        var content = _fileSystem.ReadFile(fileHandle);
        
        // TODO: add validation here - just catch exception and print error to terminal
        var machineCode = MachineCodeAssembler.ToMachineCode(content);
        _resourceManager.AddResourcePart(
            ResourceNames.ProgramInMemory,
            new ProgramInMemoryData
            {
                Name = nameof(ProgramInMemoryData),
                ProgramName = fileName,
                MachineCode = machineCode,
                IsSingleUse = true
            }
        );
    }
}