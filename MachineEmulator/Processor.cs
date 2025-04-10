﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MachineEmulator.Constants;
using MachineEmulator.Enums;
using MachineEmulator.Operations;

namespace MachineEmulator;

public class Processor : IDisposable
{
    private readonly string? _snapshotFilePath;

    public uint[] registers = new uint[12];

    private readonly RAM _ram;
    private readonly HardwareInterruptDevice _hardwareInterruptDevice;
    private readonly ExternalStorage _externalStorage;
    private readonly TimeSpan _periodicInterruptInterval;
    
    public Processor(
        RAM ram,
        HardwareInterruptDevice hardwareInterruptDevice,
        ExternalStorage externalStorage,
        TimeSpan periodicInterruptInterval,
        string? filePath = null)
    {
        _ram = ram;
        _hardwareInterruptDevice = hardwareInterruptDevice;
        _externalStorage = externalStorage;
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
        registers[(int)Register.PC] = MemoryLocations.Code;

        new Thread(WatchTerminalOutput).Start();
        new Thread(WatchKeyboardInput).Start();
        new Thread(WatchWriteToExternalStorage).Start();
        new Thread(WatchReadFromExternalStorage).Start();
        new Thread(RunPeriodicInterruptTimer).Start();
        new Thread(TrackTime).Start();
        
        while (true)
        {
            var instruction = GetDWordFromRam(registers[(int)Register.PC]);
            if (!instruction.HasValue)
                continue;
            
            registers[(int)Register.PC] += 4;

            var executeCommand = Decoder.DecodeOperation(instruction.Value);
            executeCommand(this, _ram);

            var interruptCode = _hardwareInterruptDevice.TryGetInterruptCode();
            if (interruptCode.HasValue)
            {
                MachineStateOperations.INT(this, _ram, interruptCode.Value);
            }
        }
    }

    public void SetByteInRam(ulong address, byte value)
    {
        var physicalAddress = GetPhysicalRamAddress(address);
        if (physicalAddress.HasValue)
        {
            _ram.SetByte(physicalAddress.Value, value);
        }
        else
        {
            MachineStateOperations.INT(this, _ram, InterruptCodes.PageFault);
        }
    }

    public uint? GetByteFromRam(ulong address)
    {
        var physicalAddress = GetPhysicalRamAddress(address);
        if (physicalAddress.HasValue)
        {
            return _ram.GetByte(physicalAddress.Value);
        }
        
        MachineStateOperations.INT(this, _ram, InterruptCodes.PageFault);
        return null;
    }
    
    public void SetDWordInRam(ulong address, uint value)
    {
        var physicalAddress = GetPhysicalRamAddress(address);
        if (physicalAddress.HasValue)
        {
            _ram.SetDWord(physicalAddress.Value, value);
        }
        else
        {
            MachineStateOperations.INT(this, _ram, InterruptCodes.PageFault);
        }
    }

    public uint? GetDWordFromRam(ulong address)
    {
        var physicalAddress = GetPhysicalRamAddress(address);
        if (physicalAddress.HasValue)
        {
            return _ram.GetDWord(physicalAddress.Value);
        }

        MachineStateOperations.INT(this, _ram, InterruptCodes.PageFault);
        return null;
    }
    
    public ulong? GetPhysicalRamAddress(ulong address)
    {
        if (!IsInVirtualMode)
        {
            return address;
        }

        const uint pageSize = 4096;
        var virtualPageNumber = address / pageSize;
        var pageTableLength = _ram.GetDWord(registers[(int)Register.PTBR]);
        if (virtualPageNumber > pageTableLength - 1)
        {
            return null;
        }
        
        var pageTableEntryAddress = registers[(int)Register.PTBR] + 4 + virtualPageNumber * 4;
        var pageTableEntry = _ram.GetDWord(pageTableEntryAddress);
        if ((pageTableEntry & 1) == 0)
        {
            return null;
        }

        var physicalPageNumber = pageTableEntry & 0xFFFFFFFE;
        var physicalAddress = physicalPageNumber * pageSize;
        var offsetInPage = address % pageSize;
        
        return physicalAddress + offsetInPage;
    }

    [DoesNotReturn]
    private void TrackTime() {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (true) {
            _ram.SetDWord(MemoryLocations.Time, (uint)stopwatch.Elapsed.TotalMilliseconds);
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
    private void WatchTerminalOutput()
    {
        while (true) 
        {
            var terminalValue = _ram.GetByte(MemoryLocations.TerminalOutput);
            if (terminalValue != 0) {
                Console.Write((char)terminalValue);
                _ram.SetByte(MemoryLocations.TerminalOutput, 0);
            }
        }
    }

    [DoesNotReturn]
    private void WatchKeyboardInput()
    {
        while (true)
        {
            var key = Console.ReadKey();
            _ram.SetByte(MemoryLocations.KeyboardInput, (byte)key.KeyChar);
            _hardwareInterruptDevice.Interrupt(InterruptCodes.KeyboardInput);
        }
    }

    [DoesNotReturn]
    private void WatchWriteToExternalStorage()
    {
        while (true)
        {
            var indicatorByte = _ram.GetByte(0x12003);
            if ((indicatorByte & 1) == 0)
                continue;
            
            var blockNumber = _ram.GetDWord(0x12000) & 0xFFFFFFFE;
            var data = new uint[ExternalStorage.BLOCK_SIZE];
            for (var i = 0; i < ExternalStorage.BLOCK_SIZE; i++)
                data[i] = _ram.GetDWord((uint)(0x12004 + i));
                
            _externalStorage.WriteBlock(blockNumber, data);
            _ram.SetByte(0x12003, (byte)(indicatorByte - 1));
        }
    }

    [DoesNotReturn]
    private void WatchReadFromExternalStorage()
    {
        while (true)
        {
            var indicatorByte = _ram.GetByte(0x13007);
            if ((indicatorByte & 1) == 0)
                continue;
            
            var blockNumber = _ram.GetDWord(0x13004) & 0xFFFFFFFE;
            var data = _externalStorage.ReadBlock(blockNumber);
            for (var i = 0; i < data.Length; i++)
                _ram.SetDWord((uint)(0x13008 + i), data[i]);
            
            _ram.SetByte(0x13007, (byte)(indicatorByte - 1));
        }
    }
}