using MachineEmulator.Constants;
using MachineEmulator.Enums;
using MachineEmulator.Operations;

namespace MachineEmulator
{
    static class Decoder
    {
        public static Action<Processor, RAM> DecodeOperation(uint opCode) {
            if ((opCode & 0xE0000000) == 0x20000000) { // MOV literal alternative
                Register reg = DecodeRegister((opCode & 0x1F000000) >> 24);
                uint literal = opCode & 0x00FFFFFF;
                return (proc, ram) => MemoryOperations.MOV(proc, ram, reg, literal);
            }
            if ((opCode & 0xFFFE0000) == 0x02000000) {
                return DecodeLoadOrStoreOperation(opCode);
            }
            if ((opCode & 0xFFF00000) == 0x01000000) {
                return DecodeArithmeticOperation(opCode);
            }
            if ((opCode & 0xFFF00000) == 0x01100000) {
                return DecodeLogicalOperation(opCode);
            }
            if ((opCode & 0xFFF00000) == 0x01200000) {
                return DecodeJumpOperation(opCode);
            }
            if ((opCode & 0xFFF00000) == 0x01300000) {
                return DecodeMemoryOperation(opCode);
            }
            if ((opCode & 0xFFF00000) == 0x01400000) {
                return DecodeMachineStateOperation(opCode);
            }

            return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode); 
        }

        private static Action<Processor, RAM> DecodeArithmeticOperation(uint opCode) {
            Register reg1 = DecodeRegister((opCode & 0x000003E0) >> 5);
            Register reg2 = DecodeRegister((opCode & 0x0000001F) >> 0);
            if (!IsGeneralPurposeRegister(reg1) || !IsGeneralPurposeRegister(reg2))
                return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);

            if ((opCode & 0xFFFFFC00) == 0x01000000)
                return (proc, ram) => ArithmeticOperations.ADD(proc, ram, reg1, reg2);
            if ((opCode & 0xFFFFFC00) == 0x01010000)
                return (proc, ram) => ArithmeticOperations.SUB(proc, ram, reg1, reg2);
            if ((opCode & 0xFFFFFC00) == 0x01020000)
                return (proc, ram) => ArithmeticOperations.MUL(proc, ram, reg1, reg2);
            if ((opCode & 0xFFFFFC00) == 0x01030000)
                return (proc, ram) => ArithmeticOperations.DIV(proc, ram, reg1, reg2);
            if ((opCode & 0xFFFFFC00) == 0x01040000)
                return (proc, ram) => ArithmeticOperations.CMP(proc, ram, reg1, reg2);

            return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);
        }

        private static Action<Processor, RAM> DecodeLogicalOperation(uint opCode) {
            if ((opCode & 0xFFFFFFE0) == 0x01100000) {
                Register reg = DecodeRegister((opCode & 0x0000001F) >> 0);
                if (!IsGeneralPurposeRegister(reg))
                    return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);

                return (proc, ram) => LogicalOperations.NEG(proc, ram, reg);
            }

            Register reg1 = DecodeRegister((opCode & 0x000003E0) >> 5);
            Register reg2 = DecodeRegister((opCode & 0x0000001F) >> 0);
            if (!IsGeneralPurposeRegister(reg1) || !IsGeneralPurposeRegister(reg2))
                return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);

            if ((opCode & 0xFFFFFC00) == 0x01110000)
                return (proc, ram) => LogicalOperations.AND(proc, ram, reg1, reg2);
            if ((opCode & 0xFFFFFC00) == 0x01120000)
                return (proc, ram) => LogicalOperations.OR(proc, ram, reg1, reg2);
            if ((opCode & 0xFFFFFC00) == 0x01130000)
                return (proc, ram) => LogicalOperations.XOR(proc, ram, reg1, reg2);

            return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);
        }

        private static Action<Processor, RAM> DecodeJumpOperation(uint opCode) {
            if (opCode == 0x01280000)
                return (proc, ram) => ControlFlowOperations.RET(proc, ram);

            Register reg = DecodeRegister((opCode & 0x0000001F) >> 0);
            if (!IsGeneralPurposeRegister(reg))
                return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);

            if ((opCode & 0xFFFFFFE0) == 0x01200000)
                return (proc, ram) => ControlFlowOperations.JMP(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01210000)
                return (proc, ram) => ControlFlowOperations.JC(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01220000)
                return (proc, ram) => ControlFlowOperations.JE(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01230000)
                return (proc, ram) => ControlFlowOperations.JNE(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01240000)
                return (proc, ram) => ControlFlowOperations.JNC(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01250000)
                return (proc, ram) => ControlFlowOperations.JL(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01260000)
                return (proc, ram) => ControlFlowOperations.JM(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01270000)
                return (proc, ram) => ControlFlowOperations.LOOP(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01290000)
                return (proc, ram) => ControlFlowOperations.CALL(proc, ram, reg);

            return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);
        }
        private static Action<Processor, RAM> DecodeMemoryOperation(uint opCode) {
            if (opCode == 0x01330000)
                return (proc, ram) => MemoryOperations.PUSHALL(proc, ram);
            if (opCode == 0x01340000)
                return (proc, ram) => MemoryOperations.POPALL(proc, ram);

            if ((opCode & 0xFFFFFC00) == 0x01300000) {
                Register reg1 = DecodeRegister((opCode & 0x000003E0) >> 5);
                Register reg2 = DecodeRegister((opCode & 0x0000001F) >> 0);

                if (reg1 == Register.Unknown || reg2 == Register.Unknown)
                    return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);

                return (proc, ram) => MemoryOperations.MOV(proc, ram, reg1, reg2);
            }

            Register reg = DecodeRegister((opCode & 0x0000001F) >> 0);

            if (!IsGeneralPurposeRegister(reg))
                return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);

            if((opCode & 0xFFFFFFE0) == 0x01310000)
                return (proc, ram) => MemoryOperations.PUSH(proc, ram, reg);
            if ((opCode & 0xFFFFFFE0) == 0x01320000)
                return (proc, ram) => MemoryOperations.POP(proc, ram, reg);
            
            return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);
        }

        private static Action<Processor, RAM> DecodeMachineStateOperation(uint opCode) {
            if ((opCode & 0xFFFFFF00) == 0x01400000) {
                byte code = (byte)(opCode & 0xFF);
                return (proc, ram) => MachineStateOperations.INT(proc, ram, code);
            }

            if (opCode == 0x01410000)
                return (proc, ram) => MachineStateOperations.ENTER(proc, ram);
            if (opCode == 0x01420000)
                return (proc, ram) => MachineStateOperations.EXIT(proc, ram);
            if (opCode == 0x01430000)
                return (proc, ram) => MachineStateOperations.HALT(proc, ram);

            return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);
        }
        private static Action<Processor, RAM> DecodeLoadOrStoreOperation(uint opCode) {
            Register reg1 = DecodeRegister((opCode & 0x000003E0) >> 5);
            Register reg2 = DecodeRegister((opCode & 0x0000001F) >> 0);

            if (reg1 == Register.Unknown || reg2 == Register.Unknown || reg2 == Register.FR)
                return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);

            if ((opCode & 0xFFFFFC00) == 0x02000000)
                return (proc, ram) => MemoryOperations.LOAD(proc, ram, reg1, reg2);
            if ((opCode & 0xFFFFFC00) == 0x02010000)
                return (proc, ram) => MemoryOperations.STORE(proc, ram, reg1, reg2);

            return (proc, ram) => MachineStateOperations.INT(proc, ram, InterruptCodes.InvalidOpCode);
        }

        private static Register DecodeRegister(uint registerCode) {
            switch (registerCode) {
                case 0b00000:
                    return Register.R0;
                case 0b00001:
                    return Register.R1;
                case 0b00010:
                    return Register.R2;
                case 0b00011:
                    return Register.R3;
                case 0b00100:
                    return Register.R4;
                case 0b00101:
                    return Register.R5;
                case 0b00110:
                    return Register.R6;
                case 0b00111:
                    return Register.R7;
                case 0b01000:
                    return Register.SP;
                case 0b01001:
                    return Register.PC;
                case 0b01010:
                    return Register.PTBR;
                case 0b01011:
                    return Register.FR;
                default:
                    return Register.Unknown;
            }
        }

        private static bool IsGeneralPurposeRegister(Register reg) {
            return reg != Register.Unknown && reg != Register.FR && reg != Register.PTBR;
        }
    }
}
