using PrettyApp.drawable;

namespace PrettyApp.plants;

public class Plant(int x, int y, List<PlantPart> parts) : IPixelable, IPlantPart
{
    public List<PlantPart> Parts = parts;
    public int X = x, Y = y;


    public Plant(int x, int y) : this(x, y, new())
    {
    }

    public void TickParts()
    {
        Parts.RemoveAll(p => p.ReadyToDestroy);

        foreach (PlantPart plantPart in Parts)
        {
            plantPart.TickIncludingParts();
        }
    }

    public List<(int, int, int)> GetPixelData()
    {
        List<(int, int, int)> pixelData = new();

        foreach (PlantPart plantPart in Parts)
        {
            pixelData.AddRange(plantPart.PixelData);
        }

        return pixelData;
    }

    public int GetX()
    {
        return X;
    }

    public int GetY()
    {
        return Y;
    }

    public List<PlantPart> GetParts()
    {
        return Parts;
    }

    public void TickPartsSecond()
    {
        foreach (PlantPart plantPart in Parts)
        {
            plantPart.TickIncludingPartsSecond();
        }
    }
}