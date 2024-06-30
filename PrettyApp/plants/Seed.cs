using PrettyApp.drawable;

namespace PrettyApp.plants;

public class Seed(IPlantPart parent) : PlantPart(parent)
{
    private int _counter = 25;

    public override void Tick()
    {
    }

    protected override void UpdatePixelData()
    {
        PixelDataList = new();

        if (_counter <= 5)
        {
            if (_counter >= 4)
                AddPixel(1, -1, 0x631510);
            if (_counter >= 3)
                AddPixel(0, -1, 0x631510);
            if (_counter >= 1)
                AddPixel(1, 0, 0x631510);

            AddPixel(0, 0, 0x631510);
        }
        else if (_counter <= 15)
        {
            int color = Util.LerpColor(0x00C020, 0x631510, (_counter - 5) * 0.1f);
            AddRect(0, 1, 0, -1, color);
        }
        else if (_counter <= 25)
        {
            int color = Util.LerpColor(0x30EF40, 0x00C020, (_counter - 15) * 0.1f);
            AddRect(0, 1, 0, -1, color);
        }
    }

    protected override void TickSecond()
    {
        if (_counter == 0)
        {
            ReadyToDestroy = true;
            _counter--;

            Parent.GetParts().Add(new Seedling(this));
        }
        else if (_counter >= 0)
        {
            _counter--;
            UpdateRequired = true;
        }

        Console.Out.WriteLine(_counter);
    }
}