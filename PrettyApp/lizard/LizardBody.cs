﻿using System.Drawing;
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

    public LizardBody(LizardEntity parent) : base(new Point(0, 0), GenerateSegments(7, 5, 10))
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
            AddRect((int)end.X, (int)end.Y, 5 + (i is > 2 and < 6 ? 2 : 0), 0x00BB00);
        }

        // draw head square
        AddRect((int)_segments[0].Pos.X, (int)_segments[0].Pos.Y, 7, 0x00BB00);

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
        if ((new Vector2(Goal.X, Goal.Y) - _segments[0].Pos).Length() > 0.01f)
        {
            Util.DoForwardReaching(Goal, _segments, false);

            Pos.X = (int)_segments[0].Pos.X;
            Pos.Y = (int)_segments[0].Pos.Y;

            UpdateRequired = true;
        }
    }
}