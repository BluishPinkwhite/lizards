using System.Drawing;
using System.Numerics;
using PrettyApp.util;

namespace PrettyApp.drawable;

public class SegmentLineEntity(Point pos, Segment[] segments) : Entity(pos)
{
    private Segment[] _segments = segments;
    private Vector2 _goal = new(100, 100); // the final destination the entity wants to reach
    private Vector2 _stepGoal = new(100, 100); // the eased-in step of the final goal

    private Vector2 _vel = new(14, 6);

    private float speed = 8;

    protected override void RedrawPixelData()
    {
        // AddLine(Pos.X, (int)_segments[0].Pos.X, Pos.Y, (int)_segments[0].Pos.Y, 0xEE2030);
        // AddRect(Pos.X, Pos.Y, 3, 0x3030DD);

        for (int i = 0; i < _segments.Length - 1; i++)
        {
            Vector2 start = _segments[i].Pos;
            Vector2 end = _segments[i + 1].Pos;

            AddLine(start, end, 0xEE2030);
            AddRect((int)end.X, (int)end.Y, 5, 0x00BB00);
        }

        AddRect((int)_goal.X, (int)_goal.Y, 1, 0);
        AddRect((int)_segments[0].Pos.X, (int)_segments[0].Pos.Y, 5, 0x0000BB);
        AddRect((int)(_segments[0].Pos.X + _stepGoal.X), (int)(_segments[0].Pos.Y + _stepGoal.Y), 2, 0xff0000);
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

        _goal.X += _vel.X;
        _goal.Y += _vel.Y;

        const int sizeOffset = 0;

        if (_goal.X <= -sizeOffset || _goal.X >= MainWindow.bm.PixelWidth + sizeOffset)
        {
            _vel.X *= -1;
        }

        if (_goal.Y <= -sizeOffset || _goal.Y >= MainWindow.bm.PixelHeight + sizeOffset)
        {
            _vel.Y *= -1;
        }

        if ((new Vector2(_goal.X, _goal.Y) - _segments[0].Pos).Length() > _segments[0].Length)
        {
            CalculateStepGoal();

            // Util.DoFABRIK(_segments[0].Pos, _segments[^1].Pos, _stepGoal, _segments);
            
            Util.DoForwardReaching(_segments[0].Pos + _stepGoal, _segments);
            
            UpdateRequired = true;
        }
    }

    public override void TickSecond()
    {
    }

    private void CalculateStepGoal()
    {
        Vector2 stepV = new(_segments[0].Pos.X, _segments[0].Pos.Y);

        Vector2 goalDir = Vector2.Normalize(_goal - stepV); // direction to goal from head
        _stepGoal = Vector2.Normalize(goalDir) * speed;
    }
}