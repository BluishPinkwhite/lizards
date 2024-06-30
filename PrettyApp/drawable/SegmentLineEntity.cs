using System.Drawing;
using System.Numerics;
using PrettyApp.util;

namespace PrettyApp.drawable;

public class SegmentLineEntity(Point pos, Segment[] segments) : Entity(pos)
{
    private Segment[] _segments = segments;
    private Point _goal = new(10, 10);

    protected override void RedrawPixelData()
    {
        Util.DoFABRIK(Pos, _goal, _segments);


        AddLine(Pos.X, (int)_segments[0].Pos.X, Pos.Y, (int)_segments[0].Pos.Y, 0xEE2030);
        AddRect(Pos.X, Pos.Y, 3, 0x3030DD);

        for (int i = 0; i < _segments.Length - 1; i++)
        {
            Vector2 start = _segments[i].Pos;
            Vector2 end = _segments[i + 1].Pos;

            AddLine(start, end, 0xEE2030);
            AddRect((int)end.X, (int)end.Y, 3, 0x30DD30);
        }
    }

    public override void Tick()
    {
        if (_goal.X != MainWindow.MouseX)
        {
            _goal.X = MainWindow.MouseX;
            UpdateRequired = true;
        }

        if (_goal.Y != MainWindow.MouseY)
        {
            _goal.Y = MainWindow.MouseY;
            UpdateRequired = true;
        }
    }

    public override void TickSecond()
    {
    }
}