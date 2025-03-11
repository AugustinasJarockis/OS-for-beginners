using Assembler;
using MachineEmulator;

var codeFilePath = Path.Join(Environment.CurrentDirectory, "code.txt");
var machineCode = MachineCodeAssembler.ToMachineCode(codeFilePath);
var interruptDevice = new HardwareInterruptDevice();

var ram = new RAM();
for (var i = 0; i < machineCode.Count; i++)
{
    ram.SetDWord(0x408 + i * 4, machineCode[i]);
}

var processor = new Processor(ram, interruptDevice);
processor.Run();
