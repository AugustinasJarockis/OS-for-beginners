using Assembler;
using OperatingSystem.Hardware;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.Files;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;
using Serilog;

namespace OperatingSystem.ProcessManagement.Processes;

public class StartStopProc : ProcessProgram
{
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly RAM _ram;
    private readonly ExternalStorage _externalStorage;

    private MemoryManager _memoryManager;

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
                FileSystem.Initialise(_externalStorage, _processManager, _resourceManager);
                
                _resourceManager.CreateResource(ResourceNames.OsShutdown, [], new OsShutdownScheduler());
                _resourceManager.CreateResource(ResourceNames.Focus, [], new FocusScheduler());
                _resourceManager.CreateResource(ResourceNames.Interrupt, [], new InterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.JobGovernorInterrupt, [], new JobGovernorInterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.KeyboardInput, [], new KeyboardInputScheduler());
                _resourceManager.CreateResource(ResourceNames.UserInput, [], new UserInputScheduler(_resourceManager));
                _resourceManager.CreateResource(ResourceNames.NonExistent, [], new NonExistentResourceScheduler());
                _resourceManager.CreateResource(ResourceNames.FromInterrupt, [], new FromInterruptScheduler());
                _resourceManager.CreateResource(ResourceNames.ProgramInMemory, [], new ProgramInMemoryScheduler());
                _resourceManager.CreateResource(ResourceNames.FileHandle, [], new FileHandleScheduler());
                _resourceManager.CreateResource(ResourceNames.TerminalOutput, [], new TerminalOutputScheduler());
                _resourceManager.CreateResource(ResourceNames.Focus, [], new FocusScheduler());
                
                _processManager.CreateProcess(nameof(MainProc), new MainProc(_processManager, _resourceManager, _processor, _memoryManager), isSystem: true);
                _processManager.CreateProcess(nameof(InterruptProc), new InterruptProc(_resourceManager), isSystem: true, basePriority: 32);
                _processManager.CreateProcess(nameof(IdleProc), new IdleProc(), isSystem: true);
                _processManager.CreateProcess(nameof(CLIProc), new CLIProc(_resourceManager, _processManager), isCLI: true, isSystem: true, 32);
                _processManager.CreateProcess(nameof(TerminalOutputProc), new TerminalOutputProc(_resourceManager), isSystem: true, basePriority: 64);
                _processManager.CreateProcess(nameof(KeyboardInputProc), new KeyboardInputProc(_ram, _resourceManager), isSystem: true, basePriority: 32);
                _processManager.CreateProcess(nameof(UserInputProc), new UserInputProc(_resourceManager), isSystem: true, basePriority: 32);
                
                _resourceManager.AddResourcePart(ResourceNames.Focus, new FocusData
                {
                    Name = nameof(FocusData),
                    IsSingleUse = false,
                });

                _resourceManager.SubscribeGrantedToPidChange<FocusData>(ResourceNames.Focus, (_, grantedToPid, _) =>
                {
                    Log.Information("Focused pid {Pid}", grantedToPid);
                });

                TransferDataFileToExternalStorage("halt.txt");
                TransferDataFileToExternalStorage("read_input_once.txt");
                TransferDataFileToExternalStorage("print_labas.txt");
                TransferDataFileToExternalStorage("load_store.txt");
                TransferDataFileToExternalStorage("read_input.txt");
                TransferDataFileToExternalStorage("write.txt");
                TransferDataFileToExternalStorage("letter_printer.txt");
                TransferDataFileToExternalStorage("letter_printer2.txt");
                TransferDataFileToExternalStorage("read_print.txt");

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
                Log.Information("Starting OS shutdown...");

                _processManager.KillProcess(nameof(UserInputProc));
                _processManager.KillProcess(nameof(KeyboardInputProc));
                _processManager.KillProcess(nameof(TerminalOutputProc));
                _processManager.KillProcess(nameof(CLIProc));
                _processManager.KillProcess(nameof(IdleProc));
                _processManager.KillProcess(nameof(InterruptProc));
                _processManager.KillProcess(nameof(MainProc));

                _resourceManager.DestroyAllResources();
                
                return CurrentStep + 1;
            }
            case 3:
            {
                Environment.Exit(0);
                return 3;
            }
            default:
                return 0;
        }
    }

    private void TransferDataFileToExternalStorage(string fileName)
    {
        FileSystem.CreateFile(fileName);
        _resourceManager.RequestResource(ResourceNames.FileHandle, fileName);
        var fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, fileName);
        
        var dataFilePath = Path.Join(Environment.CurrentDirectory, "Data", fileName);
        var lines = File.ReadAllLines(dataFilePath);

        FileSystem.OverwriteFile(fileHandle, lines);
        _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
    }

    private void LoadProgram(string fileName)
    {
        _resourceManager.RequestResource(ResourceNames.FileHandle, fileName);
        var fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, fileName);
        var content = FileSystem.ReadFileString(fileHandle);
        _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
        
        if (content is null)
        {
            _resourceManager.AddResourcePart(
                ResourceNames.TerminalOutput,
                new TerminalOutputData
                {
                    Name = nameof(TerminalOutputData),
                    IsSingleUse = true,
                    Text = $"Failed to read from file {fileName}",
                    ProcessId = _processManager.CurrentProcessId
                });
            return;
        }
        
        List<uint> machineCode;
        try
        {
            machineCode = MachineCodeAssembler.ToMachineCode(content);
        }
        catch (Exception ex)
        {
            _resourceManager.AddResourcePart(
                ResourceNames.TerminalOutput,
                new TerminalOutputData
                {
                    Name = nameof(TerminalOutputData),
                    IsSingleUse = true,
                    Text = $"Program {fileName} code is incorrect: {ex.Message}",
                    ProcessId = _processManager.CurrentProcessId
                });
            return;
        }
        
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