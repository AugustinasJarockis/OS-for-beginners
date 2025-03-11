using MachineEmulator.Enums;

namespace MachineEmulator.Operations
{
    class MemoryOperations
    {
        public static void LOAD(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.registers[(int)reg1] = ram.GetDWord(proc.registers[(int)reg2]);
        }
        public static void STORE(Processor proc, RAM ram, Register reg1, Register reg2) {
            ram.SetDWord(proc.registers[(int)reg2], proc.registers[(int)reg1]);
        }
        public static void MOV(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.registers[(int)reg1] = proc.registers[(int)reg2];
        }
        public static void MOV(Processor proc, RAM ram, Register reg, int literal) {
            proc.registers[(int)reg] = literal;
        }
        public static void PUSH(Processor proc, RAM ram, Register reg) {
            proc.registers[(int)Register.SP] -= 4;
            ram.SetDWord(proc.registers[(int)Register.SP], proc.registers[(int)reg]);
        }
        public static void POP(Processor proc, RAM ram, Register reg) {
            proc.registers[(int)reg] = ram.GetDWord(proc.registers[(int)Register.SP]);
            proc.registers[(int)Register.SP] += 4;
        }
        public static void PUSHALL(Processor proc, RAM ram) {
            for (int i = 0; i < 12; i++)
                PUSH(proc, ram, (Register)i);
        }
        public static void POPALL(Processor proc, RAM ram) {
            for (int i = 11; i >= 0; i--)
                POP(proc, ram, (Register)i);
        }
    }
}
