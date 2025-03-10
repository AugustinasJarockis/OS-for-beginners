using System.Diagnostics.CodeAnalysis;
using MachineEmulator.Enums;
using MachineEmulator.Operations;

namespace MachineEmulator;

public class Processor
{
    public int[] registers = new int[12];

    private readonly RAM _ram;
    private readonly HardwareInterruptDevice hardwareInterruptDevice;
    
    public Processor(RAM ram, HardwareInterruptDevice hardwareInterruptDevice)
    {
        _ram = ram;
        this.hardwareInterruptDevice = hardwareInterruptDevice;
    }

    public bool IsInVirtualMode => (registers[(int)Register.FR] & 0b0100) != 0;

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
            
            if (hardwareInterruptDevice.IsInterrupted())
            {
                var interruptCode = hardwareInterruptDevice.GetInterruptCode();
                MachineStateOperations.INT(this, _ram, interruptCode);
            }
        }
    }
}