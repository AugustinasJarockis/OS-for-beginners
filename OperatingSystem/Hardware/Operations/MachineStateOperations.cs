using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations;

public class MachineStateOperations
{
    public static void INT(Processor proc, RAM ram, byte interruptCode)
    {
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
            ram.SetDWord(MemoryLocations.RMPC, proc.registers[(int)Register.PC]);
            ram.SetDWord(MemoryLocations.RMSP, proc.registers[(int)Register.SP]);

            proc.registers[(int)Register.PC] = ram.GetDWord(MemoryLocations.VMPC);
            proc.registers[(int)Register.SP] = ram.GetDWord(MemoryLocations.VMSP);
            
            FlagUtils.SetModeFlag(proc);
        }
    }

    public static void HALT(Processor proc, RAM ram)
    {
        INT(proc, ram, InterruptCodes.Halt);
    }
}