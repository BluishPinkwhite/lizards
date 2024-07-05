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
 * Uses Forward IK to move towards goal. Other body parts are connected to it.
 *
 * _segments[0] = "neck" (first/upper segment of body)
 */
public class LizardBody : SegmentLineEntity
{
    protected LizardEntity Parent;

    // used for Tail render
    internal (Vector2, Vector2) lastSegmentPositions;

    // determines the state of tongue (hidden..., half, full, (left, full, right)..., half), state moves forward randomly
    private int _tongueState = 0;
    private Random _random = new();


    public LizardBody(LizardEntity parent) : base(new Point(0, 0), GenerateSegments(7, 5, 10))
    {
        Parent = parent;

        Pos.X = (int)_segments[0].Pos.X;
        Pos.Y = (int)_segments[0].Pos.Y;

        App.Entities.Add(this);
    }

    protected override void RedrawPixelData()
    {
        // draw body area:
        // -1st segment data
        Vector2 lastLeft, lastRight;
        {
            Vector2 segmentDir = _segments[1].Pos - _segments[0].Pos;
            Vector2 rotatedDir = Vector2.Normalize(Util.RotateVector(segmentDir, Util.PI / 2f));
            float len = _segments[0].Length - (2 / 1.3f);

            // prepare -1st segment data
            lastLeft = _segments[0].Pos + rotatedDir * -len;
            lastRight = _segments[0].Pos + rotatedDir * len;

            // prepare head
            Vector2 normSegmentDir = Vector2.Normalize(segmentDir);

            Vector2 firstLeft = _segments[0].Pos + rotatedDir * -len * 0.4f -
                                normSegmentDir * _segments[0].Length;
            Vector2 firstRight = _segments[0].Pos + rotatedDir * len * 0.4f -
                                 normSegmentDir * _segments[0].Length;

            // draw head
            FillArea(firstLeft, firstRight, lastRight, lastLeft, 0x00BB00);

            // tongue data
            if (_tongueState != 0)
            {
                Vector2 tongueBase = _segments[0].Pos - normSegmentDir * (_segments[0].Length * 1.2f);

                if (_tongueState == 3) // tongue rotated left
                    normSegmentDir = Util.RotateVector(normSegmentDir, Util.PI / 8f);
                else if (_tongueState == 5) // tongue rotated right
                    normSegmentDir = Util.RotateVector(normSegmentDir, -Util.PI / 8f);

                float lengthMul = (_tongueState == 7 || _tongueState == 1) ? 0.5f : 1;
                Vector2 tongueEnd = tongueBase - normSegmentDir * (5 * lengthMul);

                // draw tongue line
                AddLine(tongueBase, tongueEnd, 0xFF0000);

                // draw tongue side lines
                rotatedDir = Util.RotateVector(-normSegmentDir, Util.PI / 4f);
                AddLine(tongueEnd, tongueEnd + rotatedDir * 3 * lengthMul, 0xFF0000);
                rotatedDir = Util.RotateVector(-normSegmentDir, -Util.PI / 4f);
                AddLine(tongueEnd, tongueEnd + rotatedDir * 3 * lengthMul, 0xFF0000);
            }
        }

        // draw body segments
        for (int i = 1; i < _segments.Length; i++)
        {
            Vector2 segmentDir = _segments[i].Pos - _segments[i - 1].Pos;
            Vector2 rotatedDir = Vector2.Normalize(Util.RotateVector(segmentDir, Util.PI / 2f));
            float len;

            if (i <= 3)
                len = _segments[i].Length - (5 / 1.325f);
            else
                len = _segments[i].Length - (i / 1.325f);

            Vector2 left = _segments[i].Pos + rotatedDir * -len;
            Vector2 right = _segments[i].Pos + rotatedDir * len;

            FillArea(left, right, lastRight, lastLeft, 0x00BB00);

            lastLeft = left;
            lastRight = right;
        }

        // save last segments for Tail render (its -1st segment)
        lastSegmentPositions = (lastLeft, lastRight);

        // draw connecting lines
        for (int i = 0; i < _segments.Length - 1; i++)
        {
            Vector2 start = _segments[i].Pos;
            Vector2 end = _segments[i + 1].Pos;

            AddLine(start, end, 0xEE2030);
        }

        // draw eyes
        Vector2 bodyDir = _segments[0].Pos - _segments[1].Pos;
        Vector2 eye = _segments[0].Pos + 5 * Vector2.Normalize(Util.RotateVector(bodyDir, Util.PI / 4));
        AddRect((int)eye.X, (int)eye.Y, 1, 0);
        eye = _segments[0].Pos + 5 * Vector2.Normalize(Util.RotateVector(bodyDir, -Util.PI / 4));
        AddRect((int)eye.X, (int)eye.Y, 1, 0);

        // draw connecting line to tail
        AddLine(_segments[^1].Pos, Parent.Tail._segments[0].Pos, 0xEE2030);
    }

    public override void Tick()
    {
        // move goal
        if ((new Vector2(Goal.X, Goal.Y) - _segments[0].Pos).Length() > 0.01f)
        {
            Util.DoForwardReaching(Goal, _segments, false);

            Pos.X = (int)_segments[0].Pos.X;
            Pos.Y = (int)_segments[0].Pos.Y;

            UpdateRequired = true;
        }
    }


    public override void TickSecond()
    {
        // changes tongue state:
        // states: (hidden..., half, full..., (left, full(!), right)..., half)

        if (_tongueState == 0) // hidden...
        {
            if (_random.NextDouble() < 0.05)
                _tongueState++;
        }
        else if (_tongueState == 1) // half
        {
            _tongueState++;
        }
        else if (_tongueState == 2) // full...
        {
            if (_random.NextDouble() < 0.15)
                _tongueState++;
        }
        else if (_tongueState == 3) // left
        {
            _tongueState++;
        }
        else if (_tongueState == 4) // full
        {
            _tongueState++;
        }
        else if (_tongueState == 5) // right
        {
            _tongueState++;
        }
        else if (_tongueState == 6) // full!
        {
            if (_random.NextDouble() < 0.40)
                _tongueState++;
            else
                _tongueState = 3; // return to left
        }
        else if (_tongueState == 7) // half
        {
            _tongueState++;
        }
        else
        {
            _tongueState = 0; // back to none, and repeat cycle
        }
    }
}