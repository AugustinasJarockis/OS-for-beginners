using System.Diagnostics.CodeAnalysis;
using MachineEmulator.Constants;
using MachineEmulator.Enums;
using MachineEmulator.Operations;

namespace MachineEmulator;

public class Processor
{
    public uint[] registers = new uint[12];

    private readonly RAM _ram;
    private readonly HardwareInterruptDevice _hardwareInterruptDevice;
    private readonly TimeSpan _periodicInterruptInterval;
    
    public Processor(RAM ram, HardwareInterruptDevice hardwareInterruptDevice, TimeSpan periodicInterruptInterval)
    {
        _ram = ram;
        _hardwareInterruptDevice = hardwareInterruptDevice;
        _periodicInterruptInterval = periodicInterruptInterval;
    }

    public bool IsInVirtualMode => (registers[(int)Register.FR] & 0b0100) != 0;

    [DoesNotReturn]
    public void Run()
    {
        registers[(int)Register.PC] = 0x508;

        var periodicInterruptTimer = new Thread(RunPeriodicInterruptTimer);
        periodicInterruptTimer.Start();

        var outputTerminalWatcher = new Thread(WatchTerminalOutput);
        outputTerminalWatcher.Start();
        
        while (true)
        {
            var instruction = _ram.GetDWord(registers[(int)Register.PC]);
            registers[(int)Register.PC] += 4;
            
            var executeCommand = Decoder.DecodeOperation(instruction);
            executeCommand(this, _ram);

            var interruptCode = _hardwareInterruptDevice.TryGetInterruptCode();
            if (interruptCode.HasValue)
            {
                MachineStateOperations.INT(this, _ram, interruptCode.Value);
            }
        }
    }

    [DoesNotReturn]
    private void RunPeriodicInterruptTimer()
    {
        while (true)
        {
            Thread.Sleep(_periodicInterruptInterval);
            _hardwareInterruptDevice.Interrupt(InterruptCodes.PeriodicInterrupt);
        }
    }

    [DoesNotReturn]
    private void WatchTerminalOutput() {
        while(true) 
        {
            var terminalValue = _ram.GetByte(MemoryLocations.TerminalOutput);
            if (terminalValue != 0) {
                Console.Write((char)terminalValue);
                _ram.SetByte(MemoryLocations.TerminalOutput, 0);
            }
        }
    }
}