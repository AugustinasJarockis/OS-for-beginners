using System.Diagnostics.CodeAnalysis;
using MachineEmulator.Enums;
using MachineEmulator.Operations;

namespace MachineEmulator;

public class Processor
{
    public int[] registers = new int[12];

    private readonly RAM _ram;
    private readonly InterruptDevice _interruptDevice;
    
    public Processor(RAM ram, InterruptDevice interruptDevice)
    {
        _ram = ram;
        _interruptDevice = interruptDevice;
    }

    [DoesNotReturn]
    public void Run()
    {
        registers[(int)Register.PC] = 0x408;
        
        while (true)
        {
            var instruction = _ram.GetDWord(registers[(int)Register.PC]);
            registers[(int)Register.PC] += 4;
            
            var executeCommand = Decoder.DecodeOperation(instruction);
            executeCommand(this, _ram);
            
            CheckAndHandleInterruptIfNeeded();
        }
    }

    private void CheckAndHandleInterruptIfNeeded()
    {
        if (!_interruptDevice.IsInterrupted())
        {
            return;
        }
        
        if (IsInVirtualMode())
        {
            MachineStateOperations.EXIT(this, _ram);
            _ram.SetDWord(0x404, registers[(int)Register.SP]);
            registers[(int)Register.SP] = _ram.GetDWord(0x400);
        }
        
        MemoryOperations.PUSHALL(this, _ram);
        var interruptCode = _interruptDevice.GetInterruptCode();
        MachineStateOperations.INT(this, _ram, interruptCode);
    }

    private bool IsInVirtualMode() => (registers[(int)Register.FR] & 0b0100) != 0;
}