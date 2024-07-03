using System.Drawing;

namespace PrettyApp.drawable;
/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
public class LineEntity(Point pos) : Entity(pos)
{
    private int _lastX = 0, _lastY = 0;


    protected override void RedrawPixelData()
    {
        AddLine(Pos.X, _lastX, Pos.Y, _lastY, 0xEE2030);
        AddPixel(Pos.X, Pos.Y, 0x3030DD);
        AddPixel(_lastX, _lastY, 0x3030DD);
    }


    public override void Tick()
    {
        if (_lastX != MainWindow.MouseX)
        {
            _lastX = MainWindow.MouseX;
            UpdateRequired = true;
        }

        if (_lastY != MainWindow.MouseY)
        {
            _lastY = MainWindow.MouseY;
            UpdateRequired = true;
        }
    }

    public override void TickSecond()
    {
    }
}