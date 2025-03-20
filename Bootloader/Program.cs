using Assembler;
using MachineEmulator;

var programDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "vuos");
Directory.CreateDirectory(programDirectory);
var ramSnapshotFilePath = Path.Combine(programDirectory, "ramSnapshot.mem");

var ram = new RAM(ramSnapshotFilePath);
AppDomain.CurrentDomain.ProcessExit += (_, _) => ram.Dispose();

LoadInterruptHandlers(ram);
LoadCode(ram);

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

    string registerMessage = "R0: \0\0\0\0\0\0\0\0 | R1: \0\0\0\0\0\0\0\0 | R2: \0\0\0\0\0\0\0\0 | R3: \0\0\0\0\0\0\0\0 | R4: \0\0\0\0\0\0\0\0 | " + 
        "R5: \0\0\0\0\0\0\0\0 | R6: \0\0\0\0\0\0\0\0 | R7: \0\0\0\0\0\0\0\0 | " +
        "SP: \0\0\0\0\0\0\0\0 | PC: \0\0\0\0\0\0\0\0 | FR: \0\0\0\0\0\0\0\0 | PTBR: \0\0\0\0\0\0\0\0;\n\0";

    for (int i = 0; i < registerMessage.Length; i++) {
        ram.SetByte(0x10A04 + (ulong)i, (byte)registerMessage[i]);
    }
}

static void LoadCode(RAM ram)
{
    var codeFilePath = Path.Join(Environment.CurrentDirectory, "Data", "FunctionTest.txt");
    var machineCode = MachineCodeAssembler.ToMachineCode(codeFilePath);
    for (var i = 0; i < machineCode.Count; i++)
    {
        ram.SetDWord((ulong)(0x508 + i * 4), machineCode[i]);
    }
}