using Assembler;
using OperatingSystem.Hardware;
using OperatingSystem.ProcessManagement;
using OperatingSystem.ProcessManagement.Processes;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using OperatingSystem.ResourceManagement.Schedulers;

var processManager = new ProcessManager();
var resourceManager = new ResourceManager(processManager);

void OnInterrupt(byte interruptCode)
{
    var vmProc = (VMProc)processManager.CurrentProcess.Program;
    vmProc.OnInterrupt(interruptCode);
}

var ram = new RAM();
var interruptDevice = new HardwareInterruptDevice();
var externalStorage = new ExternalStorage();

var processor = new Processor(
    ram,
    interruptDevice,
    externalStorage,
    periodicInterruptInterval: TimeSpan.FromSeconds(1),
    OnInterrupt
);

processor.Start();

processManager.CreateProcess(
    nameof(StartStopProc),
    new StartStopProc(processManager, resourceManager, processor, ram)
);

var codeFilePath = Path.Join(Environment.CurrentDirectory, "Data", "test.txt");
var machineCode = MachineCodeAssembler.ToMachineCode(codeFilePath);

resourceManager.CreateResource(ResourceNames.ProgramInMemory, [
    new ProgramInMemoryData
    {
        Name = nameof(ProgramInMemoryData),
        MachineCode = machineCode,
        IsSingleUse = true
    }
], new ProgramInMemoryScheduler());

processManager.Schedule();