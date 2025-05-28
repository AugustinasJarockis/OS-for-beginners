using Assembler;
using OperatingSystem.Hardware.Enums;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.Files;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class CLIProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;
    private readonly ProcessManager _processManager;
    
    public CLIProc(ResourceManager resourceManager, ProcessManager processManager)
    {
        _resourceManager = resourceManager;
        _processManager = processManager;
        _resourceManager.SubscribeGrantedToPidChange<FocusData>(ResourceNames.Focus, OnFocusedProcessChange);
    }
    private void OnFocusedProcessChange(string _, ushort? processId, ushort? _1) => _focusedProcessId = processId;

    private ushort? _focusedProcessId;
    private List<string> _inputTokens;
    private FileHandleData fileHandle;
    int returnCase = 1;

    protected override int Next()
    {
        switch (CurrentStep) 
        {
            case 0: {
                    _resourceManager.RequestResource(ResourceNames.Focus, nameof(FocusData));
                    return CurrentStep + 1;
                }
            case 1: {
                    _resourceManager.RequestResource(ResourceNames.UserInput, nameof(UserInputData));
                    return CurrentStep + 1;
                }
            case 2: {
                    var input = _resourceManager.ReadResource<UserInputData>(ResourceNames.UserInput, nameof(UserInputData)).Text;
                    _inputTokens = input.Trim().Split(' ').Select(token => token.Trim()).ToList();

                    switch (_inputTokens[0].ToLower()) {
                        case "start": {
                                return 5;
                                }
                        case "process": {
                            return 6; 
                        }
                        case "focus": {
                            return 7; 
                        }
                        case "kill": {
                            return 8;
                        }
                        case "suspend": {
                            return 9;   
                        }
                        case "unsuspend": {
                            return 10;
                        }
                        case "shutdown": {
                            return 11;
                        }
                        case "dir": {
                            return 12;
                        }
                        case "create": {
                            return 13;
                        }
                        case "delete": {
                            returnCase = 14;
                            return 17;
                        }
                        case "write": {
                            returnCase = 15;
                            return 17;
                        }
                        case "display": {
                            returnCase = 16;
                            return 17;
                        }
                        case "registers": {
                            return 18;
                        }
                        default: {
                            return 19;
                        }
                    };
                }
            case 5: {
                    //TODO: Implement file start
                    if (_inputTokens.Count < 2 || String.IsNullOrEmpty(_inputTokens[1])) {
                        PrintMessage("Expected format: 'start [file name]'");
                        return 1;
                    }

                    LoadProgram(_inputTokens[1]);

                    return 1;
                }
            case 6: {
                    foreach (var process in _processManager.Processes) {
                        PrintMessage($"Process: {process.Name}; PID: {process.Id}; State: {process.State}");
                    }

                    return 1;
                }
            case 7: {
                    if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid)) {
                        PrintMessage("Expected format: 'focus [PID]'");
                        return 1;
                    }

                    if (!_processManager.ProcessExists(pid)) {
                        PrintMessage($"Process not found by pid {pid}");
                        return 1;
                    }

                    if (_processManager.IsSystemProcess(pid)) {
                        PrintMessage("System process cannot be focused");
                        return 1;
                    }

                    if (_processManager.IsProcessSuspended(pid))
                    {
                        PrintMessage("Suspended process cannot be focused");
                        return 1;
                    }

                    _resourceManager.ChangeOwnership<FocusData>(ResourceNames.Focus, nameof(FocusData), pid);

                    return 1;
                }
            case 8: {
                    if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid)) {
                        PrintMessage("Expected format: 'kill [PID]'");
                        return 1;
                    }

                    if (!_processManager.ProcessExists(pid)) {
                        PrintMessage($"Process not found by pid {pid}");
                        return 1;
                    }

                    if (_processManager.IsSystemProcess(pid)) {
                        PrintMessage("System process cannot be killed");
                        return 1;
                    }

                    var processName = _processManager.FindProcessById(pid).Parent!.Name;
                    _resourceManager.AddResourcePart(ResourceNames.ProgramInMemory, new ProgramInMemoryData {
                        IsSingleUse = true,
                        IsEnd = true,
                        Name = nameof(ProgramInMemoryData),
                        JobGovernorId = processName
                    });

                    return 1;
                }
            case 9: {

                if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid))
                {
                    PrintMessage("Expected format: 'suspend [PID]'");
                    return 1;
                }
                

                if (!_processManager.ProcessExists(pid))
                {
                    PrintMessage($"Process not found by pid {pid}");

                    return 1;
                }


                if (_processManager.IsSystemProcess(pid))
                {
                    PrintMessage("System process cannot be suspended");
                    return 1;
                }

                var process = _processManager.FindProcessById(pid);

                if (process.State != ProcessState.Ready && process.State != ProcessState.Blocked)
                {
                    PrintMessage($"Process {pid} is not in a suspendable state (must be READY or BLOCKED)");
                    return 1;
                }

                _processManager.SuspendProcess(pid);

                return 1;
                }
            case 10: {

                if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid))
                {
                    PrintMessage("Expected format: 'unsuspend [PID]'");
                    return 1;
                }

                if (!_processManager.ProcessExists(pid))
                {
                    PrintMessage($"Process not found by pid {pid}");
                    return 1;
                }
                
                if (_processManager.IsSystemProcess(pid))
                {
                    PrintMessage("System process cannot be unsuspended");
                    return 1;
                }

                var process = _processManager.FindProcessById(pid);

                if (process.State != ProcessState.ReadySuspended && process.State != ProcessState.BlockedSuspended)
                {
                    PrintMessage($"Process {pid} is not in a suspended state (must be READYS or BLOCKEDS)");
                    return 1;
                }

                _processManager.UnsuspendProcess(pid);

                return 1;
                }
            case 11: {
                    _resourceManager.AddResourcePart(ResourceNames.OsShutdown, new OsShutdownData {
                        Name = nameof(OsShutdownData),
                        IsSingleUse = true,
                    });
                    return 1;
                }
            case 12: {
                    var fileList = FileSystem.GetFileList();
                    PrintMessage("All files in the system: ");
                    foreach (var file in fileList) {
                        PrintMessage(file);
                    }
                    return 1;
                }
            case 13: {
                    if (_inputTokens.Count < 2 || String.IsNullOrEmpty(_inputTokens[1])) {
                        PrintMessage("Expected format: 'create [filename]'");
                        return 1;
                    }

                    fileHandle = FileSystem.CreateFile(_inputTokens[1]);
                    if (fileHandle == null) {
                        PrintMessage($"File with name {_inputTokens[1]} already exists");
                        return 1;
                    }
                    _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
                    PrintMessage($"Succesfully created file with name {_inputTokens[1]}");
                    return 1;
                }
            case 14: {
                    fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, _inputTokens[1]);

                    if (!FileSystem.DeleteFile(fileHandle)) {
                        PrintMessage($"File with name {_inputTokens[1]} was not found");
                        return 1;
                    }
                    _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
                    PrintMessage($"File with name {_inputTokens[1]} was succesfully deleted");
                    return 1;
                }
            case 15: {
                    fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, _inputTokens[1]);
                    FileSystem.OverwriteFile(fileHandle, _inputTokens.Skip(2).ToArray());
                    _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
                    return 1;
                }
            case 16: {
                    fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, _inputTokens[1]);
                    var content = FileSystem.ReadFileString(fileHandle);
                    if (content != null) {
                        foreach(var line in content) {
                            PrintMessage(line);
                        }
                    }
                    _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
                    return 1;
                }
            case 17: {
                    if (_inputTokens.Count < 2 || String.IsNullOrEmpty(_inputTokens[1])) {
                        PrintMessage($"Expected format: '{_inputTokens[0]} [filename]'");
                        return 1;
                    }

                    _resourceManager.RequestResource(ResourceNames.FileHandle, _inputTokens[1]);
                    return returnCase;
                }
            case 18:
            {
                if (_inputTokens.Count != 2 || !ushort.TryParse(_inputTokens[1], out var pid))
                {
                    PrintMessage("Expected format: 'registers [PID]'");
                    return 1;
                }

                if (!_processManager.ProcessExists(pid))
                {
                    PrintMessage($"Process not found by pid {pid}");
                    return 1;
                }

                var process = _processManager.Processes.FirstOrDefault(x => x.Id == pid);
                if (process?.Program is not VMProc vmProc)
                {
                    PrintMessage("Only can view registers of user process");
                    return 1;
                }

                PrintMessage($"Process {process.Name} registers:");
                for (var i = 0; i < 12; i++)
                {
                    PrintMessage($"{Enum.GetName(typeof(Register), i)}: {vmProc.Registers[i]}");
                }
                
                return 1;
            }
            case 19:
            {
                PrintMessage("Unknown command");    
                return 1;
            }
            default:
                return 1;
        }
    }

    private void PrintMessage(string text)
    {
        _resourceManager.AddResourcePart(ResourceNames.TerminalOutput, new TerminalOutputData
        {
            Name = nameof(TerminalOutputData),
            IsSingleUse = true,
            ProcessId = _processManager.CurrentProcessId,
            Text = text,
        });
    }

    private void LoadProgram(string fileName) {
        _resourceManager.RequestResource(ResourceNames.FileHandle, fileName);
        var fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, fileName);
        var content = FileSystem.ReadFileString(fileHandle);
        _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);

        if (content is null) {
            PrintMessage($"Failed to read from file {fileName}");
            return;
        }

        List<uint> machineCode;
        try {
            machineCode = MachineCodeAssembler.ToMachineCode(content);
        }
        catch (Exception ex) {
            PrintMessage($"Program {fileName} code is incorrect: {ex.Message}");
            return;
        }

        _resourceManager.AddResourcePart(
            ResourceNames.ProgramInMemory,
            new ProgramInMemoryData {
                Name = nameof(ProgramInMemoryData),
                ProgramName = fileName + DateTime.Now,
                MachineCode = machineCode,
                IsSingleUse = true
            }
        );
    }
}