namespace Assembler.UnitTests;

public class MachineCodeAssemblerTests
{
    [TestCase("program.txt", "program-mc.txt")]
    public void ToMachineCode_GivenInstruction_TranslatesCorrectly(
        string instructionFileName,
        string machineCodeFileName
    )
    {
        // Arrange
        var instructionFilePath = Path.Join(
            Environment.CurrentDirectory,
            "Data",
            instructionFileName
        );
        var machineCodeFilePath = Path.Join(
            Environment.CurrentDirectory,
            "Data",
            machineCodeFileName
        );
        var expectedMachineCode = ReadMachineCodeFromFile(machineCodeFilePath);

        // Act
        var actualMachineCode = MachineCodeAssembler.ToMachineCode(instructionFilePath);

        // Assert
        Assert.That(actualMachineCode, Is.EqualTo(expectedMachineCode).AsCollection);
    }

    private static List<int> ReadMachineCodeFromFile(string filePath)
    {
        List<int> machineCode = [];
        foreach (var line in File.ReadLines(filePath))
        {
            var instructionMachineCode = Convert.ToInt32(line, 2);
            machineCode.AddRange(instructionMachineCode);
        }

        return machineCode;
    }
}
