using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations
{
    class ControlFlowOperations
    {
        public static void JMP(Processor proc, RAM ram, Register reg) { // TODO: Consider out of bounds exceptions somehow?
            uint address = proc.Registers[(uint)reg];
            proc.Registers[(int)Register.PC] = address;
        }
        public static void JC(Processor proc, RAM ram, Register reg) {
            if ((proc.Registers[(int)Register.FR] & 0b1) != 0) {
                uint address = proc.Registers[(uint)reg];
                proc.Registers[(int)Register.PC] = address;
            }
        }
        public static void JE(Processor proc, RAM ram, Register reg) {
            if ((proc.Registers[(int)Register.FR] & 0b10) != 0) {
                uint address = proc.Registers[(uint)reg];
                proc.Registers[(int)Register.PC] = address;
            }
        }
        public static void JNE(Processor proc, RAM ram, Register reg) {
            if ((proc.Registers[(int)Register.FR] & 0b10) == 0) {
                uint address = proc.Registers[(uint)reg];
                proc.Registers[(int)Register.PC] = address;
            }
        }
        public static void JNC(Processor proc, RAM ram, Register reg) {
            if ((proc.Registers[(int)Register.FR] & 0b1) == 0) {
                uint address = proc.Registers[(uint)reg];
                proc.Registers[(int)Register.PC] = address;
            }
        }
        public static void JL(Processor proc, RAM ram, Register reg) {
            if ((proc.Registers[(int)Register.FR] & 0b10) == 0 && (proc.Registers[(uint)Register.FR] & 0b1000) != 0) {
                uint address = proc.Registers[(uint)reg];
                proc.Registers[(int)Register.PC] = address;
            }
        }
        public static void JM(Processor proc, RAM ram, Register reg) {
            if ((proc.Registers[(int)Register.FR] & 0b10) == 0 && (proc.Registers[(int)Register.FR] & 0b1000) == 0) {
                uint address = proc.Registers[(uint)reg];
                proc.Registers[(int)Register.PC] = address;
            }
        }
        public static void LOOP(Processor proc, RAM ram, Register reg) {
            if (proc.Registers[(int)Register.R7] != 0) {
                uint address = proc.Registers[(int)reg];
                proc.Registers[(int)Register.R7]--;
                proc.Registers[(int)Register.PC] = address;
            }
        }
        public static void RET(Processor proc, RAM ram) {
            var address = proc.GetDWordFromRam(proc.Registers[(int)Register.SP]);
            if (address.HasValue) {
                proc.Registers[(int)Register.PC] = address.Value;
                proc.Registers[(int)Register.SP] += 4;
            }
        }
        public static void CALL(Processor proc, RAM ram, Register reg) {
            MemoryOperations.PUSH(proc, ram, Register.PC);
            uint address = proc.Registers[(int)reg];
            proc.Registers[(int)Register.PC] = address;
        }
    }
}
