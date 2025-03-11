using MachineEmulator.Enums;

namespace MachineEmulator.Operations
{
    class MemoryOperations
    {
        public static void LOAD(Processor proc, RAM ram, Register reg1, Register reg2) {
            throw new NotImplementedException();
        }
        public static void STORE(Processor proc, RAM ram, Register reg1, Register reg2) {
            throw new NotImplementedException();
        }
        public static void MOV(Processor proc, RAM ram, Register reg1, Register reg2) {
            throw new NotImplementedException();
        }
        public static void MOV(Processor proc, RAM ram, Register reg, uint literal) {
            proc.registers[(int)reg] = literal;
        }
        public static void PUSH(Processor proc, RAM ram, Register reg) {
            throw new NotImplementedException();
        }
        public static void POP(Processor proc, RAM ram, Register reg) {
            throw new NotImplementedException();
        }
        public static void PUSHALL(Processor proc, RAM ram) {
            throw new NotImplementedException();
        }
        public static void POPALL(Processor proc, RAM ram) {
            throw new NotImplementedException();
        }
    }
}
