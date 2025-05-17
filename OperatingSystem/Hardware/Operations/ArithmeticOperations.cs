using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations
{
    class ArithmeticOperations
    {
        public static void ADD(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.registers[(int)reg1] + (int)proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)reg1] = (uint)result;
        }
        public static void SUB(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.registers[(int)reg1] - (int)proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)reg1] = (uint)result;
        }
        public static void MUL(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.registers[(int)reg1] * (int)proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)Register.R1] = (uint)(result / uint.MaxValue);
            proc.registers[(int)Register.R2] = (uint)(result % uint.MaxValue);
        }
        public static void DIV(Processor proc, RAM ram, Register reg1, Register reg2) {
            if (proc.registers[(int)reg2] == 0) {
                MachineStateOperations.INT(proc, ram, InterruptCodes.DivByZero);
                return;
            }
            long result = (int)proc.registers[(int)reg1] / (int)proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)reg1] = (uint)result;
        }
        public static void CMP(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = (int)proc.registers[(int)reg1] - (int)proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
        }
    }
}
