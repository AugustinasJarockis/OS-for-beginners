using MachineEmulator.Enums;

namespace MachineEmulator.Operations
{
    class LogicalOperations
    {
        public static void NEG(Processor proc, RAM ram, Register reg) {
            proc.registers[(int)reg] = ~proc.registers[(int)reg];
            FlagUtils.AdjustSignFlag(proc, proc.registers[(int)reg]);
        }
        public static void AND(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.registers[(int)reg1] & proc.registers[(int)reg2];
            FlagUtils.AdjustZeroAndSignFlags(proc, result);
            proc.registers[(int)reg1] = (uint)result;
        }
        public static void OR(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.registers[(int)reg1] | proc.registers[(int)reg2];
            FlagUtils.AdjustZeroAndSignFlags(proc, result);
            proc.registers[(int)reg1] = (uint)result;
        }
        public static void XOR(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.registers[(int)reg1] ^ proc.registers[(int)reg2];
            FlagUtils.AdjustZeroAndSignFlags(proc, result);
            proc.registers[(int)reg1] = (uint)result;
        }
    }
}
