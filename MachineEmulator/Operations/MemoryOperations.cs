using MachineEmulator.Constants;
using MachineEmulator.Enums;

namespace MachineEmulator.Operations
{
    class MemoryOperations
    {
        public static void LOAD(Processor proc, RAM ram, Register reg1, Register reg2) {
            var value = proc.GetDWordFromRam(proc.registers[(int)reg2]);
            if (value.HasValue)
                proc.registers[(int)reg1] = value.Value;
        }
        public static void STORE(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.SetDWordInRam(proc.registers[(int)reg2], proc.registers[(int)reg1]);
        }
        public static void LOADB(Processor proc, RAM ram, Register reg1, Register reg2) {
            var value = proc.GetByteFromRam(proc.registers[(int)reg2]);
            if (value.HasValue)
                proc.registers[(int)reg1] = value.Value;
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
        public static void PUSH(Processor proc, RAM ram, Register reg, bool ignoreMode = false) {
            proc.registers[(int)Register.SP] -= 4;
            
            if (ignoreMode)
                ram.SetDWord(proc.registers[(int)Register.SP], proc.registers[(int)reg]);
            else
                proc.SetDWordInRam(proc.registers[(int)Register.SP], proc.registers[(int)reg]);
        }
        public static void POP(Processor proc, RAM ram, Register reg, bool ignoreMode = false) {
            if (ignoreMode) {
                proc.registers[(int)reg] = ram.GetDWord(proc.registers[(int)Register.SP]);
            } else {
                var value = proc.GetDWordFromRam(proc.registers[(int)Register.SP]);
                if (!value.HasValue)
                    return;
                
                proc.registers[(int)reg] = value.Value;
            }
            
            proc.registers[(int)Register.SP] += 4;
        }
        public static void PUSHALL(Processor proc, RAM ram, bool ignoreMode = false) {
            for (int i = 0; i < 11; i++)
                PUSH(proc, ram, (Register)i, ignoreMode);
        }
        public static void POPALL(Processor proc, RAM ram, bool ignoreMode = false) {
            for (int i = 10; i >= 0; i--)
                POP(proc, ram, (Register)i, ignoreMode);
        }
        public static void POPINT(Processor proc, RAM ram) {
            if (proc.IsInVirtualMode) {
                MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);
            } else {
                POP(proc, ram, Register.PC);
                POPALL(proc, ram, ignoreMode: true);

                if (proc.IsInVirtualMode) {
                    proc.registers[(int)Register.PC] = ram.GetDWord(MemoryLocations.VMPC);
                    proc.registers[(int)Register.SP] = ram.GetDWord(MemoryLocations.VMSP);
                }
            }
        }
    }
}
