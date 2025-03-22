using MachineEmulator;

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

var processor = new Processor(
    ram,
    interruptDevice,
    periodicInterruptInterval: TimeSpan.FromSeconds(1),
    registerSnapshotFilePath
    );
AppDomain.CurrentDomain.ProcessExit += (_, _) => processor.Dispose();

processor.Run();