using OperatingSystem.ProcessManagement;
using OperatingSystem.ResourceManagement;

var resourceManager = new ResourceManager();
var processManager = new ProcessManager(resourceManager);

processManager.CreateProcess("notepad.txt");