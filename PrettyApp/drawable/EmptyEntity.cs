using System.Drawing;
using PrettyApp.util;

namespace PrettyApp.drawable;

public class EmptyEntity : Entity
{
    public EmptyEntity(Point pos, List<Pixel> pixelData) : base(pos)
    {
        PixelData = pixelData;
    }

    public EmptyEntity(Point pos) : base(pos)
    {
    }

    protected override void RedrawPixelData()
    {
        // do nothing
    }

    public override void Tick()
    {
        // do nothing
    }

    public override void TickSecond()
    {
        // do nothing
    }
}