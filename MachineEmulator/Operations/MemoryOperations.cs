using MachineEmulator.Enums;

namespace MachineEmulator.Operations
{
    class MemoryOperations
    {
        public static void LOAD(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.registers[(int)reg1] = proc.GetDWordFromRam(proc.registers[(int)reg2])!.Value;
        }
        public static void STORE(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.SetDWordInRam(proc.registers[(int)reg2], proc.registers[(int)reg1]);
        }
        public static void LOADB(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.registers[(int)reg1] = proc.GetByteFromRam(proc.registers[(int)reg2])!.Value;
        }
        public static void STOREB(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.SetByteInRam(proc.registers[(int)reg2], (byte)(proc.registers[(int)reg1] & 0xFF));
        }
        public static void MOV(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.registers[(int)reg1] = proc.registers[(int)reg2];
        }
        public static void MOVD(Processor proc, RAM ram, Register reg, uint literal) {
            proc.registers[(int)reg] = literal;
        }
        public static void PUSH(Processor proc, RAM ram, Register reg) {
            proc.registers[(int)Register.SP] -= 4;
            proc.SetDWordInRam(proc.registers[(int)Register.SP], proc.registers[(int)reg]);
        }
        public static void POP(Processor proc, RAM ram, Register reg) {
            proc.registers[(int)reg] = proc.GetDWordFromRam(proc.registers[(int)Register.SP])!.Value;
            proc.registers[(int)Register.SP] += 4;
        }
        public static void PUSHALL(Processor proc, RAM ram) {
            for (int i = 0; i < 11; i++)
                PUSH(proc, ram, (Register)i);
        }
        public static void POPALL(Processor proc, RAM ram) {
            for (int i = 10; i >= 0; i--)
                POP(proc, ram, (Register)i);
        }
        public static void POPINT(Processor proc, RAM ram) {
            POP(proc, ram, Register.PC);
            POPALL(proc, ram);
            
            if (proc.IsInVirtualMode)
                proc.registers[(int)Register.SP] = ram.GetDWord(0x404);
        }
    }
}
