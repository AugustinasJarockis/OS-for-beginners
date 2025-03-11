using MachineEmulator.Constants;
using MachineEmulator.Enums;

namespace MachineEmulator.Operations;

public class MachineStateOperations
{
    public static void INT(Processor proc, RAM ram, byte interruptCode)
    {
        if (proc.IsInVirtualMode)
        {
            EXIT(proc, ram);
        }
        
        MemoryOperations.PUSHALL(proc, ram);
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
            FlagUtils.SetModeFlag(proc);
        }
    }

    public static void EXIT(Processor proc, RAM ram)
    {
        if (proc.IsInVirtualMode)
        {
            ram.SetDWord(0x404, proc.registers[(int)Register.SP]);
            proc.registers[(int)Register.SP] = ram.GetDWord(0x400);
            FlagUtils.ClearModeFlag(proc);
        }
        else
        {
            INT(proc, ram, InterruptCodes.InvalidOpCode);
        }
    }
        
    public static void HALT(Processor proc, RAM ram)
    {
        EXIT(proc, ram);
        // TODO: do something more; maybe call an interrupt?
    }
}