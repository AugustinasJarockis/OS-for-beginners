using OperatingSystem.ProcessManagement;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.Resources;
using OperatingSystem.ResourceManagement.Schedulers;

List<MemoryPart> memoryParts = [];
var memoryScheduler = new MemoryScheduler();

var resourceManager = new ResourceManager();
resourceManager.CreateResource(ResourceNames.Memory, memoryParts, memoryScheduler);

var processManager = new ProcessManager(resourceManager);

processManager.CreateProcess("notepad.txt");