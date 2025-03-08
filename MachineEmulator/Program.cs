using Assembler;
using MachineEmulator;

var codeFilePath = Path.Join(Environment.CurrentDirectory, "code.txt");
var machineCode = MachineCodeAssembler.ToMachineCode(codeFilePath);

var ram = new RAM();
for (var i = 0; i < machineCode.Count; i++)
{
    ram[0x408 + i] = machineCode[i];
}

ram[0] = 0xFF;
ram[1] = 0xEE;
ram[2] = 0xDD;
ram[3] = 0xCC;
ram[4] = 0xBB;
Console.WriteLine(ram[0] + " | " + ram[1] + " | " + ram[2] + " | " + ram[3] + " | " + ram[4]);

var processor = new Processor(ram);
processor.Run();
