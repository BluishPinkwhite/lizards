using System.Drawing;
using System.Numerics;
using PrettyApp.util;

namespace PrettyApp.drawable;

public class SegmentLineEntity(Point pos, Segment[] segments) : Entity(pos)
{
    private Segment[] _segments = segments;
    private Point _goal = new(100, 100);
    private Point _vel = new(7,3);

    protected override void RedrawPixelData()
    {
        Util.DoBackwardReaching(_goal, _segments);

        // AddLine(Pos.X, (int)_segments[0].Pos.X, Pos.Y, (int)_segments[0].Pos.Y, 0xEE2030);
        // AddRect(Pos.X, Pos.Y, 3, 0x3030DD);
        AddRect((int)_segments[0].Pos.X, (int)_segments[0].Pos.Y, 5, 0x00BB00);

        for (int i = 0; i < _segments.Length - 1; i++)
        {
            Vector2 start = _segments[i].Pos;
            Vector2 end = _segments[i + 1].Pos;

            AddLine(start, end, 0xEE2030);
            AddRect((int)end.X, (int)end.Y, 5, 0x00BB00);
        }
        
        AddRect((int)_goal.X, (int)_goal.Y, 1, 0);
    }

    public override void Tick()
    {
        // if (_goal.X != MainWindow.MouseX)
        // {
        //     _goal.X = MainWindow.MouseX;
        //     UpdateRequired = true;
        // }
        //
        // if (_goal.Y != MainWindow.MouseY)
        // {
        //     _goal.Y = MainWindow.MouseY;
        //     UpdateRequired = true;
        // }

        _goal.X += _vel.X;
        _goal.Y += _vel.Y;

        if (_goal.X <= 0 || _goal.X >= MainWindow.bm.PixelWidth)
        {
            _vel.X *= -1;
        }
        if (_goal.Y <= 0 || _goal.Y >= MainWindow.bm.PixelHeight)
        {
            _vel.Y *= -1;
        }
        
        UpdateRequired = true;
    }

    public override void TickSecond()
    {
    }
}