using PrettyApp.drawable;

namespace PrettyApp.plants;

public class Seedling(IPlantPart parent) : PlantPart(parent)
{
    private int _counter = 25;
    
    public override void Tick()
    {
        
    }

    protected override void UpdatePixelData()
    {
        PixelDataList = new();

        AddPixel(0,-25-_counter, 0xFFFFFF);
    }

    protected override void TickSecond()
    {
        UpdateRequired = true;
        _counter--;
        Console.Out.WriteLine(_counter);
    }
}