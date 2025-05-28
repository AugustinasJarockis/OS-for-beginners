using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.Files;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class JobGovernorProc : ProcessProgram
{
    private const int DataSizeInBytes = 16384; // 16KB
    private const int StackSizeInBytes = 32768; // 32KB

    private readonly string _programName;
    private readonly List<uint> _machineCode;
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly MemoryManager _memoryManager;
    private readonly uint[] _registers = new uint[12];

    private ushort _vmPid;
    private string _vmName;
    private JobGovernorInterruptData _interruptData;

    private string fileName;
    private Dictionary<uint, (WriteMode, FileHandleData)> fileHandles;
    private uint nextFileHandleKey = 1;
    private WriteMode writeMode = WriteMode.Unknown;

    public JobGovernorProc(
        string programName,
        List<uint> machineCode,
        ProcessManager processManager,
        ResourceManager resourceManager,
        Processor processor,
        MemoryManager memoryManager)
    {
        _programName = programName;
        _machineCode = machineCode;
        _processManager = processManager;
        _resourceManager = resourceManager;
        _processor = processor;
        _memoryManager = memoryManager;
    }

    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                var programSizeInBytes = _machineCode.Count * 4;
                _memoryManager.AllocateMemory(programSizeInBytes + DataSizeInBytes + StackSizeInBytes);
                
                return CurrentStep + 1;
            }
            case 1:
            {
                for (var i = 0; i < _machineCode.Count; i++)
                    _memoryManager.SetDWord((ulong) i * 4, _machineCode[i]);

                _registers[(int)Register.SP] = (uint)_machineCode.Count * 4;
                _registers[(int)Register.PTBR] = _memoryManager.GetPageTableAddress(_processManager.CurrentProcessId);
                
                _vmName = $"{nameof(VMProc)}_{_programName}";
                _vmPid = _processManager.CreateProcess(
                    _vmName,
                    new VMProc(_programName, _resourceManager, _processor, _registers)
                );

                return CurrentStep + 1;
            }
            case 2:
            {
                _resourceManager.RequestResource(
                    ResourceNames.JobGovernorInterrupt,
                    $"{nameof(JobGovernorInterruptData)}_{_programName}"
                );

                return CurrentStep + 1;
            }
            case 3:
            {
                _interruptData = _resourceManager.ReadResource<JobGovernorInterruptData>(
                    ResourceNames.JobGovernorInterrupt,
                    $"{nameof(JobGovernorInterruptData)}_{_programName}"
                );
                
                if (_interruptData.InterruptCode is
                    InterruptCodes.DivByZero or
                    InterruptCodes.InvalidOpCode or 
                    InterruptCodes.PageFault or
                    InterruptCodes.Halt)
                {
                    return 5;
                }
                
                if (_interruptData.InterruptCode == InterruptCodes.TerminalOutput)
                {
                    var str = _memoryManager.GetStringUntilZero(_registers[(int)Register.R3]);
                    
                    _resourceManager.AddResourcePart(ResourceNames.TerminalOutput, new TerminalOutputData
                    {
                        Name = nameof(TerminalOutputData),
                        IsSingleUse = true,
                        ProcessId = _vmPid,
                        Text = str
                    });
                }
                else if (_interruptData.InterruptCode == InterruptCodes.WriteToExternalStorage)
                {
                }
                else if (_interruptData.InterruptCode == InterruptCodes.ReadFromExternalStorage)
                {

                }
                else if (_interruptData.InterruptCode == InterruptCodes.GetFileHandle) 
                {
                    fileName = _memoryManager.GetStringUntilZero(_registers[(int)Register.R1]);

                    var r5 = _registers[(int)Register.R5];

                    if (r5 == 1 || r5 == 4) // TODO: potencialus bugas su blogais write modais kai bandoma atidaryti kelis failus
                        writeMode = WriteMode.Append;
                    else
                        writeMode = WriteMode.Begin;
    
                    if (FileSystem.FileExists(fileName)) {
                        if (r5 == 0 || r5 == 1 || r5 == 3 || r5 == 4) {
                            _resourceManager.RequestResource(ResourceNames.FileHandle, fileName);
                            return 8;
                        }
                    }
                    else {
                        if (r5 == 2 || r5 == 3 || r5 == 4) {
                            var fileHandle = FileSystem.CreateFile(fileName)!;
                            TrackFileHandle(fileHandle);
                            return 2;
                        }
                    }

                    _registers[(int)Register.R2] = 0;
                        return 2;
                }
                else if (_interruptData.InterruptCode == InterruptCodes.ReleaseFileHandle) 
                {
                    var fileHandle = fileHandles[_registers[(int)Register.R2]].Item2;
                    fileHandles.Remove(_registers[(int)Register.R2]);
                    _resourceManager.ReleaseResourcePart(ResourceNames.FileHandle, fileHandle);
                    return 2;
                }
                else if (_interruptData.InterruptCode == InterruptCodes.ReadKeyboardInput) {
                    _resourceManager.RequestResource(ResourceNames.UserInput, nameof(UserInputData));
                    return 7;
                }

                return CurrentStep + 1;
            }
            case 4:
            {
                _processManager.ActivateProcess(_vmPid);
                
                _resourceManager.AddResourcePart(
                    ResourceNames.FromInterrupt,
                    new FromInterruptData
                    {
                        Name = $"{nameof(FromInterruptData)}_{_programName}",
                        IsSingleUse = true,
                    });

                return 2;
            }
            case 5:
            {
                _resourceManager.AddResourcePart(
                    ResourceNames.ProgramInMemory,
                    new ProgramInMemoryData
                    {
                        Name = nameof(ProgramInMemoryData),
                        JobGovernorId = $"{nameof(JobGovernorProc)}_{_programName}",
                        IsSingleUse = true,
                        IsEnd = true,
                    });
                
                _resourceManager.RequestResource(ResourceNames.NonExistent, string.Empty);
                
                return CurrentStep + 1;
            }
            case 6:
            {
                return 6;
            }
            case 7:
            {
                var userInput = _resourceManager.ReadResource<UserInputData>(ResourceNames.UserInput, nameof(UserInputData));
                var address = _registers[(int)Register.R3];
                foreach (var ch in userInput.Text)
                {
                    if (ch == '\n')
                        break;

                    var value = (byte)ch;
                    _memoryManager.SetByte(address++, value);
                }
                
                return 4;
            }
            case 8: 
            {
                var fileHandle = _resourceManager.ReadResource<FileHandleData>(ResourceNames.FileHandle, fileName);
                TrackFileHandle(fileHandle);
                return 2;
            }
            default:
                return 5;
        }
    }

    private void TrackFileHandle(FileHandleData fileHandle) {
        fileHandles.Add(nextFileHandleKey, (writeMode, fileHandle));
        _registers[(int)Register.R2] = nextFileHandleKey;
        nextFileHandleKey++;
        if (nextFileHandleKey == 0)
            nextFileHandleKey++;
    }
}