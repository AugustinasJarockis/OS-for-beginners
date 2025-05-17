using OperatingSystem.Hardware;
using OperatingSystem.ProcessManagement;
using OperatingSystem.ProcessManagement.Processes;
using OperatingSystem.ResourceManagement;

var ramSnapshotFilePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "vuos",
    "ramSnapshot.mem");
var registerSnapshotFilePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "vuos",
    "registerSnapshot.mem");

var ram = new RAM(ramSnapshotFilePath);
AppDomain.CurrentDomain.ProcessExit += (_, _) => ram.Dispose();

var interruptDevice = new HardwareInterruptDevice();
var externalStorage = new ExternalStorage();

var processor = new Processor(
    ram,
    interruptDevice,
    externalStorage,
    periodicInterruptInterval: TimeSpan.FromSeconds(1),
    registerSnapshotFilePath
);
AppDomain.CurrentDomain.ProcessExit += (_, _) => processor.Dispose();

processor.Run();

var processManager = new ProcessManager();
var resourceManager = new ResourceManager(processManager);

processManager.CreateProcess(
    nameof(StartStopProc),
    new StartStopProc(processManager, resourceManager)
);

processManager.Schedule();