using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations
{
    class ArithmeticOperations
    {
        public static void ADD(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.Registers[(int)reg1] + (int)proc.Registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.Registers[(int)reg1] = (uint)result;
        }
        public static void SUB(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.Registers[(int)reg1] - (int)proc.Registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.Registers[(int)reg1] = (uint)result;
        }
        public static void MUL(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.Registers[(int)reg1] * (int)proc.Registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.Registers[(int)Register.R1] = (uint)(result / uint.MaxValue);
            proc.Registers[(int)Register.R2] = (uint)(result % uint.MaxValue);
        }
        public static void DIV(Processor proc, RAM ram, Register reg1, Register reg2) {
            if (proc.Registers[(int)reg2] == 0) {
                MachineStateOperations.INT(proc, ram, InterruptCodes.DivByZero);
                return;
            }
            long result = (int)proc.Registers[(int)reg1] / (int)proc.Registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.Registers[(int)reg1] = (uint)result;
        }
        public static void CMP(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.Registers[(int)reg1] - (int)proc.Registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
        }
    }
}
