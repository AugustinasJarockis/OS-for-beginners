﻿using Assembler;
using MachineEmulator;

// var ram = new RAM("ramSnapshot.mem");
var ram = new RAM();
AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => ram.Dispose();

LoadInterruptHandlers(ram);
LoadCode(ram);

var interruptDevice = new HardwareInterruptDevice();

var processor = new Processor(
    ram,
    interruptDevice,
    periodicInterruptInterval: TimeSpan.FromSeconds(10));
processor.Run();

static void LoadInterruptHandlers(RAM ram)
{
    var addressByInterruptCode = new Dictionary<byte, uint>
    {
        [0] = 0x10000,
        [1] = 0x10100,
        [2] = 0x10200,
        [3] = 0x10300,
        [4] = 0x10400,
        [5] = 0x10500,
        [6] = 0x10600,
        [7] = 0x10700
    };

    foreach (var (interruptCode, handlerAddress) in addressByInterruptCode)
    {
        ram.SetDWord((ulong)(interruptCode * 4), handlerAddress);
        
        var handlerFilePath = Path.Join(Environment.CurrentDirectory, "Data", "InterruptHandlers", $"{interruptCode}.txt");
        var machineCode = MachineCodeAssembler.ToMachineCode(handlerFilePath);
        for (var i = 0; i < machineCode.Count; i++)
        {
            ram.SetDWord((ulong)(handlerAddress + i * 4), machineCode[i]);
        }
    }
}

static void LoadCode(RAM ram)
{
    var codeFilePath = Path.Join(Environment.CurrentDirectory, "Data", "code.txt");
    var machineCode = MachineCodeAssembler.ToMachineCode(codeFilePath);
    for (var i = 0; i < machineCode.Count; i++)
    {
        ram.SetDWord((ulong)(0x508 + i * 4), machineCode[i]);
    }
}