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
 * Attached to sides of certain middle segments of Body.
 * Uses full FABRIK - forward and backward
 *
 * Goal = Shoulder location (point attached to body)
 * Pos = Foot location (point attached to ground), moves when needed to step forward
 */
public class LizardLeg : SegmentLineEntity
{
    protected LizardEntity Parent;

    // Body segment index to attach to
    private int _attachSegment;

    // angle along body to stretch leg towards
    private float _rotationAngle;

    // sum of segment lengths
    private readonly int _lengthSum;

    // handles separate movement of legs (so they take turns to step over), handled by LizardEntity.tick
    internal bool canMoveFoot = true;

    // determines how far should the leg stretch towards desired foot location (0 no stretch, 1 fully)
    private readonly float _footStretch = 0.75f;

    internal float currentStretch { get; private set; } = 0.7f;


    public LizardLeg(LizardEntity parent, int attachSegment, float rotationAngle) :
        base(new Point(0, 0), GenerateSegments(4, 180, 7))
    {
        Parent = parent;
        _attachSegment = attachSegment;
        _rotationAngle = rotationAngle;
        _lengthSum = (int)_segments.Sum(segment => segment.Length);
        canMoveFoot = rotationAngle > 0;

        Pos.X = (int)_segments[0].Pos.X;
        Pos.Y = (int)_segments[0].Pos.Y;

        _segments[0].AngleFreedom = 120;

        App.Entities.Add(this);
    }

    protected override void RedrawPixelData()
    {
        Vector2 lastLeft, lastRight;
        // -1st segment data
        {
            Vector2 segmentDir = _segments[0].Pos - Parent.Body._segments[_attachSegment].Pos;
            Vector2 rotatedDir = Vector2.Normalize(Util.RotateVector(segmentDir, Util.PI / 2f));
            float len = _segments[0].Length * 0.35f;

            // prepare -1st segment data
            lastLeft = Parent.Body._segments[_attachSegment].Pos + rotatedDir * -len;
            lastRight = Parent.Body._segments[_attachSegment].Pos + rotatedDir * len;
        }

        // draw leg parts
        for (int i = _segments.Length - 2; i >= 0; i--)
        {
            Vector2 segmentDir = _segments[i].Pos - _segments[i + 1].Pos;
            Vector2 rotatedDir = Vector2.Normalize(Util.RotateVector(segmentDir, Util.PI / 2f));
            float len = _segments[i].Length * 0.35f;

            Vector2 left = _segments[i].Pos + rotatedDir * -len;
            Vector2 right = _segments[i].Pos + rotatedDir * len;

            FillArea(left, right, lastRight, lastLeft, 0x00DD40);

            lastLeft = left;
            lastRight = right;
        }

        // draw base square
        AddRect((int)_segments[0].Pos.X, (int)_segments[0].Pos.Y, 1, 0x00BB00);


        // draw connecting lines
        // for (int i = 0; i < _segments.Length - 1; i++)
        // {
        //     Vector2 start = _segments[i].Pos;
        //     Vector2 end = _segments[i + 1].Pos;
        //
        //     AddLine(start, end, 0xFFFF00);
        // }


        // desired foot location
        // Vector2 bodyDir = Vector2.Normalize(Parent.Body._segments[_attachSegment].Pos -
        //                                     Parent.Body._segments[_attachSegment - 1].Pos);
        // Vector2 leg = Util.RotateVector(bodyDir, _rotationAngle);
        // leg = Vector2.Reflect(leg, bodyDir);
        // leg = Parent.Body._segments[_attachSegment].Pos + leg * _lengthSum * _footStretch;
        // AddRect((int)leg.X, (int)leg.Y, 1, 0x00FFFF);


        // current foot location
        // AddRect(Pos.X, Pos.Y, 1, 0xFF00FF);
    }

    public override void Tick()
    {
        // update shoulder attachment location
        Goal = Parent.Body._segments[_attachSegment].Pos;

        Vector2 currentFootLocation = new Vector2(Pos.X, Pos.Y);

        if ((currentFootLocation - Parent.Body._segments[_attachSegment].Pos).Length() > 0.01f)
        {
            // move segments
            Util.DoFABRIK(currentFootLocation, Goal, Vector2.Zero, _segments);
            
            // update position (for bounding box calculation start location)
            Pos.X = (int)_segments[0].Pos.X;
            Pos.Y = (int)_segments[0].Pos.Y;

            // redraw entity
            UpdateRequired = true;
        }
    }

    public override void TickSecond()
    {
        float bodyDistance = (_segments[0].Pos - Parent.Body._segments[_attachSegment].Pos).Length();

        Vector2 bodyDirection = Vector2.Normalize(Parent.Body._segments[_attachSegment].Pos -
                                                  Parent.Body._segments[_attachSegment - 1].Pos);

        // check if to update foot attachment location
        Vector2 currentFootLocation = new Vector2(Pos.X, Pos.Y);
        Vector2 desiredFootLocation = Util.RotateVector(bodyDirection, _rotationAngle);
        desiredFootLocation = Vector2.Normalize(Vector2.Reflect(desiredFootLocation, bodyDirection));
        desiredFootLocation =
            Parent.Body._segments[_attachSegment].Pos + desiredFootLocation * _lengthSum * _footStretch;

        // move feet when foot location is too far
        float footDistance = (desiredFootLocation - currentFootLocation).Length();

        if (canMoveFoot && (footDistance > _lengthSum * _footStretch / 2f || bodyDistance > 5f))
        {
            canMoveFoot = false;

            Pos.X = (int)desiredFootLocation.X;
            Pos.Y = (int)desiredFootLocation.Y;

            UpdateRequired = true;
        }

        // distance of current-desired foot location + shoulder-spine distance
        currentStretch = (footDistance + bodyDistance) / _lengthSum;
    }
}