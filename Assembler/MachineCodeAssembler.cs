namespace Assembler;

public static class MachineCodeAssembler
{
    public static List<uint> ToMachineCode(string filePath)
    {
        List<uint> machineCode = [];

        foreach (var instruction in File.ReadLines(filePath))
        {
            var instructionMachineCode = InstructionToMachineCode(instruction);
            machineCode.AddRange(instructionMachineCode);
        }

        return machineCode;
    }

    private static uint InstructionToMachineCode(string instruction)
    {
        var (mnemonic, rest) = ParseNextInstructionValue(instruction);
        var machineCode = MnemonicToMachineCode(mnemonic) << 16;
        var totalOperandCount = GetInstructionOperandCount(mnemonic);

        for (uint i = 1; i <= totalOperandCount; i++)
        {
            var (operand, _rest) = ParseNextInstructionValue(rest);
            
            machineCode |= OperandToMachineCode(
                mnemonic,
                operand,
                currentOperandPosition: i,
                totalOperandCount
            );
            
            rest = _rest;
        }

        return machineCode;
    }

    private static uint OperandToMachineCode(
        string mnemonic,
        string operand,
        uint currentOperandPosition,
        uint totalOperandCount
    ) =>
        mnemonic switch
        {
            "INT" => uint.Parse(operand),
            "MOVD" => currentOperandPosition == 1
                ? RegisterToMachineCode(operand) << 24
                : uint.Parse(operand),
            _ => RegisterToMachineCode(operand) << (int)(5 * (totalOperandCount - currentOperandPosition)),
        };

    private static (string, string) ParseNextInstructionValue(string instruction)
    {
        var trimmedInstruction = instruction.Trim();
        var firstSpaceIndex = trimmedInstruction.IndexOf(' ');
        if (firstSpaceIndex == -1)
        {
            return (trimmedInstruction.ToUpper(), string.Empty);
        }

        var mnemonic = trimmedInstruction[..firstSpaceIndex];
        var rest = trimmedInstruction[(firstSpaceIndex + 1)..];

        return (mnemonic.ToUpper(), rest.Trim());
    }

    private static uint GetInstructionOperandCount(string mnemonic) =>
        mnemonic switch
        {
            "RET" or "PUSHALL" or "POPALL" or "ENTER" or "EXIT" or "POPINT" => 0,
            "NEG"
            or "JMP"
            or "JC"
            or "JE"
            or "JNE"
            or "JNC"
            or "JL"
            or "JM"
            or "LOOP"
            or "CALL"
            or "PUSH"
            or "POP"
            or "INT" => 1,
            "ADD"
            or "SUB"
            or "MUL"
            or "DIV"
            or "CMP"
            or "AND"
            or "OR"
            or "XOR"
            or "LOAD"
            or "STORE"
            or "LOADB"
            or "STOREB"
            or "MOV"
            or "MOVD" => 2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(mnemonic),
                $"Invalid mnemonic: {mnemonic}"
            ),
        };

    private static uint MnemonicToMachineCode(string mnemonic) =>
        mnemonic switch
        {
            "ADD" => 0b0000_0001_0000_0000,
            "SUB" => 0b0000_0001_0000_0001,
            "MUL" => 0b0000_0001_0000_0010,
            "DIV" => 0b0000_0001_0000_0011,
            "CMP" => 0b0000_0001_0000_0100,
            "NEG" => 0b0000_0001_0001_0000,
            "AND" => 0b0000_0001_0001_0001,
            "OR" => 0b0000_0001_0001_0010,
            "XOR" => 0b0000_0001_0001_0011,
            "JMP" => 0b0000_0001_0010_0000,
            "JC" => 0b0000_0001_0010_0001,
            "JE" => 0b0000_0001_0010_0010,
            "JNE" => 0b0000_0001_0010_0011,
            "JNC" => 0b0000_0001_0010_0100,
            "JL" => 0b0000_0001_0010_0101,
            "JM" => 0b0000_0001_0010_0110,
            "LOOP" => 0b0000_0001_0010_0111,
            "RET" => 0b0000_0001_0010_1000,
            "CALL" => 0b0000_0001_0010_1001,
            "LOAD" => 0b0000_0010_0000_0000,
            "STORE" => 0b0000_0010_0000_0001,
            "LOADB" => 0b0000_0010_0000_0010,
            "STOREB" => 0b0000_0010_0000_0011,
            "MOV" => 0b0000_0001_0011_0000,
            "MOVD" => 0b0010_0000_0000_0000,
            "PUSH" => 0b0000_0001_0011_0001,
            "POP" => 0b0000_0001_0011_0010,
            "PUSHALL" => 0b0000_0001_0011_0011,
            "POPALL" => 0b0000_0001_0011_0100,
            "POPINT" => 0b0000_0001_0011_0101,
            "INT" => 0b0000_0001_0100_0000,
            "ENTER" => 0b0000_0001_0100_0001,
            "EXIT" => 0b0000_0001_0100_0010,
            _ => throw new ArgumentOutOfRangeException(
                nameof(mnemonic),
                $"Invalid mnemonic: {mnemonic}"
            ),
        };

    private static uint RegisterToMachineCode(string register) =>
        register switch
        {
            "R0" => 0b0000,
            "R1" => 0b0001,
            "R2" => 0b0010,
            "R3" => 0b0011,
            "R4" => 0b0100,
            "R5" => 0b0101,
            "R6" => 0b0110,
            "R7" => 0b0111,
            "SP" => 0b1000,
            "PC" => 0b1001,
            "PTBR" => 0b1010,
            "FR" => 0b1011,
            _ => throw new ArgumentOutOfRangeException(
                nameof(register),
                $"Invalid register: {register}"
            ),
        };
}
