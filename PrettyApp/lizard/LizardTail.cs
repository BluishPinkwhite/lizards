using System.Drawing;
using System.Numerics;
using PrettyApp.drawable;
using PrettyApp.util;

namespace PrettyApp.lizard;

/**
 * Connected at the end of Body. (first segment of tail follows last segment of body using Forward IK)
 * 
 */
public class LizardTail : SegmentLineEntity
{
    protected LizardEntity Parent;
    
    public LizardTail(LizardEntity parent) : base(new Point(0, 0), GenerateSegments(8, 15, 6))
    {
        Parent = parent;
        
        Pos.X = (int)_segments[0].Pos.X;
        Pos.Y = (int)_segments[0].Pos.Y;

        _segments[0].AngleFreedom = 180;
        
        App.Entities.Add(this);
    }
    
    protected override void RedrawPixelData()
    {
        // draw tail squares
        for (int i = 1; i < _segments.Length; i++)
        {
            Vector2 end = _segments[i].Pos;
            AddRect((int)end.X, (int)end.Y, 5 - (i/2), 0x00BB00);
        }
        
        // draw base square
        AddRect((int)_segments[0].Pos.X, (int)_segments[0].Pos.Y, 5, 0x0066BB);
        
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
        Goal = Parent.Body._segments[^1].Pos;
        
        if ((new Vector2(Goal.X, Goal.Y) - _segments[0].Pos).Length() > 0.01f)
        {
            // Util.DoFABRIK(_segments[0].Pos, _segments[^1].Pos, _stepGoal, _segments);

            Util.DoForwardReaching(Goal, _segments, false);
            
            Pos.X = (int)_segments[0].Pos.X;
            Pos.Y = (int)_segments[0].Pos.Y;

            UpdateRequired = true;
        }
    }
}