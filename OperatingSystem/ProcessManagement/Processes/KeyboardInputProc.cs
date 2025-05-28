using OperatingSystem.Hardware;
using OperatingSystem.Hardware.Constants;
using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class KeyboardInputProc : ProcessProgram
{
    private readonly RAM _ram;
    private readonly ResourceManager _resourceManager;

    public KeyboardInputProc(RAM ram, ResourceManager resourceManager)
    {
        _ram = ram;
        _resourceManager = resourceManager;
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                var key = (char)_ram.GetByte(MemoryLocations.KeyboardInput);
                if (key == 0)
                    return 0;

                lock (HardwareDevices.KeyboardInputLock)
                {
                    _ram.SetByte(MemoryLocations.KeyboardInput, 0);
                    _resourceManager.AddResourcePart(
                        ResourceNames.KeyboardInput,
                        new KeyboardInputData
                        {
                            PressedKey = key,
                            Name = nameof(KeyboardInputData),
                            IsSingleUse = true,
                        });
                }
                
                return 0;
            }
            default:
                return 0;
        }
    }
}