namespace Assembler.UnitTests;

public class MachineCodeAssemblerTests
{
    [TestCase("program.txt", "program-mc.txt")]
    public async Task ToMachineCode_GivenInstruction_TranslatesCorrectly(
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
        var expectedMachineCode = await ReadMachineCodeFromFile(machineCodeFilePath);

        // Act
        var actualMachineCode = await MachineCodeAssembler.ToMachineCode(instructionFilePath);

        // Assert
        Assert.That(actualMachineCode, Is.EqualTo(expectedMachineCode).AsCollection);
    }

    private static async Task<List<int>> ReadMachineCodeFromFile(string filePath)
    {
        List<int> machineCode = [];
        await foreach (var line in File.ReadLinesAsync(filePath))
        {
            var instructionMachineCode = Convert.ToInt32(line, 2);
            machineCode.Add(instructionMachineCode);
        }

        return machineCode;
    }
}
