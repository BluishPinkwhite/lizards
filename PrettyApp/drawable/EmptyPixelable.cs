namespace PrettyApp.drawable;

public class EmptyPixelable(List<(int, int, int)> pixelData) : IPixelable
{
    public List<(int, int, int)> GetPixelData()
    {
        return PixelData;
    }

    public List<(int, int, int)> PixelData { get; } = pixelData;
}