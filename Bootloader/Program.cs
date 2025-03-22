using Assembler;
using MachineEmulator;

var programDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "vuos");
Directory.CreateDirectory(programDirectory);
var ramSnapshotFilePath = Path.Combine(programDirectory, "ramSnapshot.mem");

var ram = new RAM(ramSnapshotFilePath, startClear: true);
AppDomain.CurrentDomain.ProcessExit += (_, _) => ram.Dispose();

LoadInterruptHandlers(ram);
LoadCode(ram, "VmHaltTest.txt", withVm: true);

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
        [7] = 0x10700,
        [8] = 0x10800,
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

    string invalidOpCode = "Page fault...\n";
    for (int i = 0; i < invalidOpCode.Length; i++) {
        ram.SetByte(0x10350 + (ulong)i, (byte)invalidOpCode[i]);
    }

    string periodicMessage = "Periodic interrupt...\n";
    for (int i = 0; i < periodicMessage.Length; i++) {
        ram.SetByte(0x10550 + (ulong)i, (byte)periodicMessage[i]);
    }

    string registerMessage = "R0: \0\0\0\0\0\0\0\0 | R1: \0\0\0\0\0\0\0\0 | R2: \0\0\0\0\0\0\0\0 | R3: \0\0\0\0\0\0\0\0 | R4: \0\0\0\0\0\0\0\0 | " + 
        "R5: \0\0\0\0\0\0\0\0 | R6: \0\0\0\0\0\0\0\0 | R7: \0\0\0\0\0\0\0\0 | " +
        "SP: \0\0\0\0\0\0\0\0 | PC: \0\0\0\0\0\0\0\0 | FR: \0\0\0\0\0\0\0\0 | PTBR: \0\0\0\0\0\0\0\0;\n\0";

    for (int i = 0; i < registerMessage.Length; i++) {
        ram.SetByte(0x10A04 + (ulong)i, (byte)registerMessage[i]);
    }
}

static void LoadCode(RAM ram, string fileName, bool withVm = false)
{
    if (withVm)
    {
        var rmFilePath = Path.Join(Environment.CurrentDirectory, "Data", "RmForVm.txt");
        var rmMachineCode = MachineCodeAssembler.ToMachineCode(rmFilePath);
        for (var i = 0; i < rmMachineCode.Count; i++)
        {
            ram.SetDWord((ulong)(0x508 + i * 4), rmMachineCode[i]);
        }
        
        var vmFilePath = Path.Join(Environment.CurrentDirectory, "Data", fileName);
        var vmMachineCode = MachineCodeAssembler.ToMachineCode(vmFilePath);
        for (var i = 0; i < vmMachineCode.Count; i++)
        {
            ram.SetDWord((ulong)(0x40000000 + i * 4), vmMachineCode[i]);
        }
    }
    else
    {
        var codeFilePath = Path.Join(Environment.CurrentDirectory, "Data", fileName);
        var machineCode = MachineCodeAssembler.ToMachineCode(codeFilePath);
        for (var i = 0; i < machineCode.Count; i++)
        {
            ram.SetDWord((ulong)(0x508 + i * 4), machineCode[i]);
        }
    }
}