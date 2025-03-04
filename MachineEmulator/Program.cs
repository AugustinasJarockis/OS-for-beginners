using MachineEmulator;

var ram = new RAM(data: []);
var processor = new Processor(ram);
processor.Run();
