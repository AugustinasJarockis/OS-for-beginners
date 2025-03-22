using MachineEmulator.Constants;
using MachineEmulator.Enums;

namespace MachineEmulator.Operations;

public class MachineStateOperations
{
    public static void INT(Processor proc, RAM ram, byte interruptCode)
    {
        var r3Override = proc.registers[(int)Register.R3];
        if (interruptCode == InterruptCodes.TerminalOutput)
        {
            var address = proc.GetPhysicalRamAddress(proc.registers[(int)Register.R3]);
            if (!address.HasValue)
                return;
            
            r3Override = (uint)address.Value;
        }

        if (proc.IsInVirtualMode)
        {
            ram.SetDWord(0x396, proc.registers[(int)Register.PC]);
            ram.SetDWord(0x404, proc.registers[(int)Register.SP]);
            proc.registers[(int)Register.SP] = ram.GetDWord(0x400);
        }
        
        MemoryOperations.PUSHALL(proc, ram, ignoreMode: true);
        MemoryOperations.PUSH(proc, ram, Register.PC, ignoreMode: true);
        FlagUtils.ClearModeFlag(proc);
        
        proc.registers[(int)Register.R3] = r3Override;
        proc.registers[(int)Register.PC] = ram.GetDWord(4 * (ulong)interruptCode);
    }
        
    public static void ENTER(Processor proc, RAM ram)
    {
        if (proc.IsInVirtualMode)
        {
            INT(proc, ram, InterruptCodes.InvalidOpCode);
        }
        else
        {
            proc.registers[(int)Register.PC] = ram.GetDWord(0x396);
            FlagUtils.SetModeFlag(proc);
        }
    }

    public static void HALT(Processor proc, RAM ram)
    {
        if (proc.IsInVirtualMode)
        {
            proc.registers[(int)Register.PC] = ram.GetDWord(0x392);
            proc.registers[(int)Register.SP] = ram.GetDWord(0x400);
            FlagUtils.ClearModeFlag(proc);
        }
        else
        {
            INT(proc, ram, InterruptCodes.InvalidOpCode);
        }
    }
}