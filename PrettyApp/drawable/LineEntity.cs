namespace PrettyApp.drawable;

public class LineEntity : Entity
{
    private int LastX = 0, LastY = 0;
    
    
    public LineEntity(int x, int y, List<(int, int, int)> pixelData) : base(x, y, pixelData)
    {
    }

    public LineEntity(int x, int y) : base(x, y)
    {
    }

    protected override void RedrawPixelData()
    {
        AddLine(X,LastX,Y, LastY, 0xEE2030);
        AddPixel(X,Y,0x3030DD);
        AddPixel(LastX, LastY, 0x3030DD);
    }
    

    public override void Tick()
    {
        if (LastX != MainWindow.MouseX)
        {
            LastX = MainWindow.MouseX;
            UpdateRequired = true;
        }
        if (LastY != MainWindow.MouseY)
        {
            LastY = MainWindow.MouseY;
            UpdateRequired = true;
        }

        Console.Out.WriteLine("\tTick");
    }

    public override void TickSecond()
    {
        
    }
    
    
}