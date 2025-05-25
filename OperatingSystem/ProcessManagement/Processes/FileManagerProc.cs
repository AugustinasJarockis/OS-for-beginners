using OperatingSystem.ResourceManagement;
using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ProcessManagement.Processes;

public class FileManagerProc : ProcessProgram
{
    private readonly ResourceManager _resourceManager;

    private FileOperationData _fileOperationData;

    public FileManagerProc(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    protected override int Next()
    {
        switch (CurrentStep)
        {
            case 0:
            {
                _resourceManager.RequestResource(ResourceNames.FileOperation, nameof(FileOperationData));
                return CurrentStep + 1;
            }
            case 1:
            {
                _fileOperationData = _resourceManager.ReadResource<FileOperationData>(
                    ResourceNames.FileOperation,
                    nameof(FileOperationData)
                );

                return 0;
            }
            default:
                return 0;
        }
    }
}