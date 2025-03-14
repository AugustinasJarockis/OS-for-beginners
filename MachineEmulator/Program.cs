using Assembler;
using MachineEmulator;

var ram = new RAM("ramSnapshot.mem");
//var ram = new RAM();
AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => ram.Dispose();
var codeFilePath = Path.Join(Environment.CurrentDirectory, "code.txt");
var machineCode = MachineCodeAssembler.ToMachineCode(codeFilePath);
var interruptDevice = new HardwareInterruptDevice();

for (var i = 0; i < machineCode.Count; i++)
{
    ram.SetDWord((ulong)(0x508 + i * 4), machineCode[i]);
}

var processor = new Processor(ram, interruptDevice);
processor.Run();
