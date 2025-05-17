using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class InterruptProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;

    private InterruptData _interruptData;
    
    public InterruptProc(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }
    
    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.RequestResource(ResourceNames.Interrupt, nameof(InterruptData));
                return CurrentStep + 1;   
            }
            case 1:
            {
                _interruptData = _resourceManager.ReadResource<InterruptData>(ResourceNames.Interrupt, nameof(InterruptData));
                return CurrentStep + 1;   
            }
            case 2:
            {
                var data = new JobGovernorInterruptData
                {
                    Name = $"{nameof(JobGovernorInterruptData)}_{_interruptData.JobGovernorGuid}",
                    InterruptCode = _interruptData.InterruptCode,
                    IsSingleUse = true
                };
                
                _resourceManager.AddResourcePart(ResourceNames.JobGovernorInterrupt, data);

                return 0;
            }
            default:
                return 0;
        }
    }
}