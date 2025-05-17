using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations
{
    class LogicalOperations
    {
        public static void NEG(Processor proc, RAM ram, Register reg) {
            proc.Registers[(int)reg] = ~proc.Registers[(int)reg];
            FlagUtils.AdjustSignFlag(proc, proc.Registers[(int)reg]);
        }
        public static void AND(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.Registers[(int)reg1] & proc.Registers[(int)reg2];
            FlagUtils.AdjustZeroAndSignFlags(proc, result);
            proc.Registers[(int)reg1] = (uint)result;
        }
        public static void OR(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.Registers[(int)reg1] | proc.Registers[(int)reg2];
            FlagUtils.AdjustZeroAndSignFlags(proc, result);
            proc.Registers[(int)reg1] = (uint)result;
        }
        public static void XOR(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.Registers[(int)reg1] ^ proc.Registers[(int)reg2];
            FlagUtils.AdjustZeroAndSignFlags(proc, result);
            proc.Registers[(int)reg1] = (uint)result;
        }
    }
}
