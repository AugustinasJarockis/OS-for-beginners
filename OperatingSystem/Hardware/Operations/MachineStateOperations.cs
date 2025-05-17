using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations;

public class MachineStateOperations
{
    public static void INT(Processor proc, RAM ram, byte interruptCode)
    {
        var r3Override = proc.Registers[(int)Register.R3];
        if (interruptCode
            is InterruptCodes.TerminalOutput
            or InterruptCodes.WriteToExternalStorage
            or InterruptCodes.ReadFromExternalStorage)
        {
            var address = proc.GetPhysicalRamAddress(proc.Registers[(int)Register.R3]);
            if (!address.HasValue)
                return;
            
            r3Override = (uint)address.Value;
        }

        if (proc.IsInVirtualMode)
        {
            ram.SetDWord(MemoryLocations.VMPC, proc.Registers[(int)Register.PC]);
            ram.SetDWord(MemoryLocations.VMSP, proc.Registers[(int)Register.SP]);
            proc.Registers[(int)Register.SP] = ram.GetDWord(MemoryLocations.RMSP);
        }
        
        MemoryOperations.PUSHALL(proc, ram, ignoreMode: true);
        MemoryOperations.PUSH(proc, ram, Register.PC, ignoreMode: true);
        FlagUtils.ClearModeFlag(proc);
        
        proc.Registers[(int)Register.R3] = r3Override;
        proc.Registers[(int)Register.PC] = ram.GetDWord(4 * (ulong)interruptCode);

        proc.OnInterrupt(interruptCode);
    }
        
    public static void ENTER(Processor proc, RAM ram)
    {
        if (proc.IsInVirtualMode)
        {
            INT(proc, ram, InterruptCodes.InvalidOpCode);
        }
        else
        {
            ram.SetDWord(MemoryLocations.RMPC, proc.Registers[(int)Register.PC]);
            ram.SetDWord(MemoryLocations.RMSP, proc.Registers[(int)Register.SP]);

            proc.Registers[(int)Register.PC] = ram.GetDWord(MemoryLocations.VMPC);
            proc.Registers[(int)Register.SP] = ram.GetDWord(MemoryLocations.VMSP);
            
            FlagUtils.SetModeFlag(proc);
        }
    }

    public static void HALT(Processor proc, RAM ram)
    {
        if (proc.IsInVirtualMode)
        {
            proc.Registers[(int)Register.PC] = ram.GetDWord(MemoryLocations.RMPC);
            proc.Registers[(int)Register.SP] = ram.GetDWord(MemoryLocations.RMSP);
            FlagUtils.ClearModeFlag(proc);
        }
        else
        {
            INT(proc, ram, InterruptCodes.InvalidOpCode);
        }
    }
}