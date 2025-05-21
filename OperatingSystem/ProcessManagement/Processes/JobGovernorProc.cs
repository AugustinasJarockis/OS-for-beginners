using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;
using OperatingSystem.Hardware.Operations;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class JobGovernorProc : ProcessProgram
{
    private readonly Guid _guid;
    private readonly List<uint> _machineCode;
    private readonly ProcessManager _processManager;
    private readonly ResourceManager _resourceManager;
    private readonly Processor _processor;
    private readonly RAM _ram;

    private ushort _vmPid;
    private JobGovernorInterruptData _interruptData;

    public JobGovernorProc(
        Guid guid,
        List<uint> machineCode,
        ProcessManager processManager,
        ResourceManager resourceManager,
        Processor processor,
        RAM ram)
    {
        _guid = guid;
        _machineCode = machineCode;
        _processManager = processManager;
        _resourceManager = resourceManager;
        _processor = processor;
        _ram = ram;
    }

    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                // TODO: assign resources to the VM; copy machine code into memory
                for (var i = 0; i < _machineCode.Count; i++)
                    _ram.SetDWord((ulong)i * 4, _machineCode[i]);
                
                _vmPid = _processManager.CreateProcess(
                    $"{nameof(VMProc)}_{_guid}",
                    new VMProc(_guid, _resourceManager, _processor)
                );

                // TODO: we should not use RAM directly - we should request for memory from resource manager
                MachineStateOperations.ENTER(_processor, _ram);
                
                return CurrentStep + 1;
            }
            case 1:
            {
                _resourceManager.RequestResource(
                    ResourceNames.JobGovernorInterrupt,
                    $"{nameof(JobGovernorInterruptData)}_{_guid}"
                );

                return CurrentStep + 1;
            }
            case 2:
            {
                _interruptData = _resourceManager.ReadResource<JobGovernorInterruptData>(
                    ResourceNames.JobGovernorInterrupt,
                    $"{nameof(JobGovernorInterruptData)}_{_guid}"
                );
                
                _processManager.SuspendProcess(_vmPid);
                
                // Console.WriteLine($"Interrupt occurred: {_interruptData.InterruptCode}");

                if (_interruptData.InterruptCode == InterruptCodes.KeyboardInput)
                {
                    var key = (char)_ram.GetByte(MemoryLocations.KeyboardInput);
                    _ram.SetByte(MemoryLocations.KeyboardInput, 0);
                    _resourceManager.AddResourcePart(
                        ResourceNames.KeyboardInput,
                        new KeyboardInputData
                        {
                            PressedKey = key,
                            Name = nameof(KeyboardInputData),
                            IsSingleUse = true,
                        });
                }
                
                // TODO: request resources needed for interrupt here

                return CurrentStep + 1;
            }
            case 3:
            {
                // TODO: read granted resources needed for interrupt handling
                
                // TODO: if we failed to handle interrupt, stop the VM
                
                _processManager.ActivateProcess(_vmPid);
                
                return 1;
            }
            default:
                return 0;
        }
    }
}