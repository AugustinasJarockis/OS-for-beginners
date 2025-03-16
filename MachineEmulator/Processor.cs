using System.Diagnostics.CodeAnalysis;
using MachineEmulator.Constants;
using MachineEmulator.Enums;
using MachineEmulator.Operations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MachineEmulator;

public class Processor : IDisposable
{
    private readonly string? _snapshotFilePath;

    public uint[] registers = new uint[12];

    private readonly RAM _ram;
    private readonly HardwareInterruptDevice _hardwareInterruptDevice;
    private readonly TimeSpan _periodicInterruptInterval;
    
    public Processor(RAM ram, HardwareInterruptDevice hardwareInterruptDevice, TimeSpan periodicInterruptInterval, string? filePath = null)
    {
        _ram = ram;
        _hardwareInterruptDevice = hardwareInterruptDevice;
        _periodicInterruptInterval = periodicInterruptInterval;
        _snapshotFilePath = filePath;
        if (filePath != null && File.Exists(filePath)) {
            byte[] fileData = File.ReadAllBytes(filePath);
            Buffer.BlockCopy(fileData, 0, registers, 0, sizeof(uint) * registers.Length);
        }
    }

    public void Dispose() {
        if (_snapshotFilePath is not null) {
            FileStream stream = File.Open(_snapshotFilePath, FileMode.Create);
            byte[] writeData = new byte[sizeof(uint) * registers.Length];
            Buffer.BlockCopy(registers, 0, writeData, 0, sizeof(uint) * registers.Length);
            stream.Write(writeData, 0, sizeof(uint) * registers.Length);
            stream.Close();
        }
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