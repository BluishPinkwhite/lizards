using System.Drawing;
using System.Numerics;
using PrettyApp.util;

namespace PrettyApp.drawable;

public class SegmentLineEntity(Point pos, Segment[] segments) : Entity(pos)
{
    protected internal Segment[] _segments = segments;
    public Vector2 Goal = new(100, 100);


    protected override void RedrawPixelData()
    {
        for (int i = 0; i < _segments.Length - 1; i++)
        {
            Vector2 start = _segments[i].Pos;
            Vector2 end = _segments[i + 1].Pos;

            AddLine(start, end, 0xEE2030);
            AddRect((int)end.X, (int)end.Y, 5, 0x00BB00);
        }

        AddRect((int)_segments[0].Pos.X, (int)_segments[0].Pos.Y, 5, 0x0000BB);
    }

    public override void Tick()
    {
        // if ((int)_goal.X != MainWindow.MouseX)
        // {
        //     _goal.X = MainWindow.MouseX;
        //     UpdateRequired = true;
        // }
        //
        // if ((int)_goal.Y != MainWindow.MouseY)
        // {
        //     _goal.Y = MainWindow.MouseY;
        //     UpdateRequired = true;
        // }

        if ((new Vector2(Goal.X, Goal.Y) - _segments[0].Pos).Length() > 0)
        {
            // Util.DoFABRIK(_segments[0].Pos, _segments[^1].Pos, _stepGoal, _segments);

            Util.DoForwardReaching(Goal, _segments, false);

            UpdateRequired = true;
        }


        Pos.X = (int)_segments[0].Pos.X;
        Pos.Y = (int)_segments[0].Pos.Y;
    }

    public override void TickSecond()
    {
    }

    protected static Segment[] GenerateSegments(int amount, int angleFreedom, int len)
    {
        Segment[] segments = new Segment[amount];
        int d = 0;

        for (int i = 0; i < amount; i++)
        {
            segments[i] = new Segment(new Vector2((d++) * len, 0), len, angleFreedom);
        }

        return segments;
    }
}