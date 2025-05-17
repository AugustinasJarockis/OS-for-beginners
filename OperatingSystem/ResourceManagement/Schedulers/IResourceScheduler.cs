using OperatingSystem.ResourceManagement.ResourceParts;

namespace OperatingSystem.ResourceManagement.Schedulers;

public interface IResourceScheduler<TPart> where TPart : ResourcePart
{
    List<ushort> Run(Resource<TPart> resource);
}