using System.Drawing;
using System.Numerics;
using PrettyApp.drawable;
using PrettyApp.util;

namespace PrettyApp.lizard;

public class LizardBody : SegmentLineEntity
{
    protected LizardEntity Parent;
    
    public LizardBody(LizardEntity parent) : base(new Point(0, 0), GenerateSegments(10, 5, 10))
    {
        Parent = parent;
        
        Pos.X = (int)_segments[0].Pos.X;
        Pos.Y = (int)_segments[0].Pos.Y;
        
        App.Entities.Add(this);
    }

    protected override void RedrawPixelData()
    {
        // draw body squares
        for (int i = 1; i < _segments.Length; i++)
        {
            Vector2 end = _segments[i].Pos;
            AddRect((int)end.X, (int)end.Y, 5, 0x00BB00);
        }
        
        // draw head square
        AddRect((int)_segments[0].Pos.X, (int)_segments[0].Pos.Y, 5, 0x0000BB);
        
        // draw connecting lines
        for (int i = 0; i < _segments.Length - 1; i++)
        {
            Vector2 start = _segments[i].Pos;
            Vector2 end = _segments[i + 1].Pos;

            AddLine(start, end, 0xEE2030);
        }
    }

    public override void Tick()
    {
        if ((new Vector2(Goal.X, Goal.Y) - _segments[0].Pos).Length() >  0.01f)
        {
            // Util.DoFABRIK(_segments[0].Pos, _segments[^1].Pos, _stepGoal, _segments);

            Util.DoForwardReaching(Goal, _segments, false);
            
            Pos.X = (int)_segments[0].Pos.X;
            Pos.Y = (int)_segments[0].Pos.Y;

            UpdateRequired = true;
        }
    }
}