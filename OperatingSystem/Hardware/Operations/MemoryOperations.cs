using OperatingSystem.Hardware.Constants;
using OperatingSystem.Hardware.Enums;

namespace OperatingSystem.Hardware.Operations
{
    class MemoryOperations
    {
        public static void LOAD(Processor proc, RAM ram, Register reg1, Register reg2) {
            var value = proc.GetDWordFromRam(proc.Registers[(int)reg2]);
            if (value.HasValue)
                proc.Registers[(int)reg1] = value.Value;
        }
        public static void STORE(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.SetDWordInRam(proc.Registers[(int)reg2], proc.Registers[(int)reg1]);
        }
        public static void LOADB(Processor proc, RAM ram, Register reg1, Register reg2) {
            var value = proc.GetByteFromRam(proc.Registers[(int)reg2]);
            if (value.HasValue)
                proc.Registers[(int)reg1] = value.Value;
        }
        public static void STOREB(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.SetByteInRam(proc.Registers[(int)reg2], (byte)(proc.Registers[(int)reg1] & 0xFF));
        }
        public static void MOV(Processor proc, RAM ram, Register reg1, Register reg2) {
            proc.Registers[(int)reg1] = proc.Registers[(int)reg2];
        }
        public static void MOVD(Processor proc, RAM ram, Register reg, uint literal) {
            proc.Registers[(int)reg] = literal;
        }
        public static void PUSH(Processor proc, RAM ram, Register reg, bool ignoreMode = false) {
            proc.Registers[(int)Register.SP] -= 4;
            
            if (ignoreMode)
                ram.SetDWord(proc.Registers[(int)Register.SP], proc.Registers[(int)reg]);
            else
                proc.SetDWordInRam(proc.Registers[(int)Register.SP], proc.Registers[(int)reg]);
        }
        public static void POP(Processor proc, RAM ram, Register reg, bool ignoreMode = false) {
            if (ignoreMode) {
                proc.Registers[(int)reg] = ram.GetDWord(proc.Registers[(int)Register.SP]);
            } else {
                var value = proc.GetDWordFromRam(proc.Registers[(int)Register.SP]);
                if (!value.HasValue)
                    return;
                
                proc.Registers[(int)reg] = value.Value;
            }
            
            proc.Registers[(int)Register.SP] += 4;
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
                    proc.Registers[(int)Register.PC] = ram.GetDWord(MemoryLocations.VMPC);
                    proc.Registers[(int)Register.SP] = ram.GetDWord(MemoryLocations.VMSP);
                }
            }
        }
    }
}
