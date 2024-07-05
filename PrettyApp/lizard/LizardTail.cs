using System.Drawing;
using System.Numerics;
using PrettyApp.drawable;
using PrettyApp.util;
using PrettyApp.window;

namespace PrettyApp.lizard;

/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
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

        _segments[0].AngleFreedom = 120;

        App.Entities.Add(this);
    }

    protected override void RedrawPixelData()
    {
        // draw tail area:
        // -1st segment data
        (Vector2 lastLeft, Vector2 lastRight) = Parent.Body.lastSegmentPositions;

        // draw
        for (int i = 1; i < _segments.Length; i++)
        {
            Vector2 segmentDir = _segments[i].Pos - _segments[i - 1].Pos;
            Vector2 rotatedDir = Vector2.Normalize(Util.RotateVector(segmentDir, Util.PI / 2f));
            float len = _segments[i].Length - (i / 1.325f);

            Vector2 left = _segments[i].Pos + rotatedDir * -len;
            Vector2 right = _segments[i].Pos + rotatedDir * len;

            FillArea(left, right, lastRight, lastLeft, 0x00BB00);

            lastLeft = left;
            lastRight = right;
        }

        // draw connecting lines
        for (int i = 0; i < _segments.Length - 1; i++)
        {
            Vector2 start = _segments[i].Pos;
            Vector2 end = _segments[i + 1].Pos;

            AddLine(start, end, 0xEE2030);
        }

        // draw connecting line to body
        AddLine(_segments[0].Pos, Parent.Body._segments[^1].Pos, 0xEE2030);
    }

    public override void Tick()
    {
        Goal = Parent.Body._segments[^1].Pos;

        if ((new Vector2(Goal.X, Goal.Y) - _segments[0].Pos).Length() > 0.01f)
        {
            Util.DoForwardReaching(Goal, _segments, false);

            Pos.X = (int)_segments[0].Pos.X;
            Pos.Y = (int)_segments[0].Pos.Y;

            UpdateRequired = true;
        }
    }
}