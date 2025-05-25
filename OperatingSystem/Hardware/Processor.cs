using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;
using OperatingSystem.Hardware.Operations;

namespace OperatingSystem.Hardware;

public class Processor
{
    public readonly uint[] registers = new uint[12];
    public readonly Action<byte> OnInterrupt;

    private readonly RAM _ram;
    
    public Processor(RAM ram, Action<byte> onInterrupt)
    {
        _ram = ram;
        OnInterrupt = onInterrupt;
    }

    public bool IsInVirtualMode => (registers[(int)Register.FR] & 0b0100) != 0;

    public void UpdateRegisters(uint[] newRegisters)
    {
        for (var i = 0; i < registers.Length; i++)
        {
            registers[i] = newRegisters[i];
        }
    }

    public void Step()
    {
        var instruction = GetDWordFromRam(registers[(int)Register.PC]);
        if (!instruction.HasValue)
            return;
        
        registers[(int)Register.PC] += 4;
    
        var executeCommand = Decoder.DecodeOperation(instruction.Value);
        executeCommand(this, _ram);
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

        var physicalPageNumber = (pageTableEntry & 0xFFFFFFFE) >> 1;
        var physicalAddress = physicalPageNumber * pageSize;
        var offsetInPage = address % pageSize;
        
        return physicalAddress + offsetInPage;
    }
}