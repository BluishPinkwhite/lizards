using PrettyApp.plants;

namespace PrettyApp.drawable;

public interface IPlantPart : IPositionable
{
    public List<PlantPart> GetParts();
}