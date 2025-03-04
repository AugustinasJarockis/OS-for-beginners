using MachineEmulator;

var ram = new RAM();

ram[0] = 0xFF;
ram[1] = 0xEE;
ram[2] = 0xDD;
ram[3] = 0xCC;
ram[4] = 0xBB;
Console.WriteLine(ram[0] + " | " + ram[1] + " | " + ram[2] + " | " + ram[3] + " | " + ram[4]);

var processor = new Processor(ram);
processor.Run();
