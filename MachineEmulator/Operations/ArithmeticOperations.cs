using MachineEmulator.Enums;

namespace MachineEmulator.Operations
{
    class ArithmeticOperations
    {
        public static void ADD(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.registers[(int)reg1] + proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)reg1] = (int)result;
        }
        public static void SUB(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.registers[(int)reg1] - proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)reg1] = (int)result;
        }
        public static void MUL(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.registers[(int)reg1] * proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)reg1] = (int)result;
        }
        public static void DIV(Processor proc, RAM ram, Register reg1, Register reg2) {
            if (proc.registers[(int)reg2] == 0) {
                throw new NotImplementedException(); //TODO: IMPLEMENT DIV IS NULIO
                return;
            }
            long result = proc.registers[(int)reg1] / proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
            proc.registers[(int)reg1] = (int)result;
        }
        public static void CMP(Processor proc, RAM ram, Register reg1, Register reg2) {
            long result = proc.registers[(int)reg1] - proc.registers[(int)reg2];
            FlagUtils.AdjustResultFlags(proc, result);
        }
    }
}
