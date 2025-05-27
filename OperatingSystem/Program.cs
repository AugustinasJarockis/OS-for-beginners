using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.ProcessManagement;
using OperatingSystem.ProcessManagement.Processes;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Information()
    .CreateLogger();

var processManager = new ProcessManager();
var resourceManager = new ResourceManager(processManager);

var ram = new RAM();
var externalStorage = new ExternalStorage();

HardwareDevices.WatchTerminalOutput(ram);
HardwareDevices.WatchKeyboardInput(ram, () =>
{
    var key = (char)ram.GetByte(MemoryLocations.KeyboardInput);
    ram.SetByte(MemoryLocations.KeyboardInput, 0);
    resourceManager.AddResourcePart(
        ResourceNames.KeyboardInput,
        new KeyboardInputData
        {
            PressedKey = key,
            Name = nameof(KeyboardInputData),
            IsSingleUse = true,
        });
});
HardwareDevices.WatchWriteToExternalStorage(ram, externalStorage);
HardwareDevices.WatchReadFromExternalStorage(ram, externalStorage);
HardwareDevices.TrackTime(ram);
HardwareDevices.RunPeriodicInterruptTimer(TimeSpan.FromMilliseconds(50), () =>
{
    processManager.HandlePeriodicInterrupt();
});

void OnVMInterrupt(byte interruptCode)
{
    var vmProc = (VMProc)processManager.CurrentProcess.Program;
    vmProc.OnInterrupt(interruptCode);
}

var processor = new Processor(ram, OnVMInterrupt);

processManager.CreateProcess(
    nameof(StartStopProc),
    new StartStopProc(processManager, resourceManager, processor, ram, externalStorage),
    isSystem: true
);

processManager.Schedule();