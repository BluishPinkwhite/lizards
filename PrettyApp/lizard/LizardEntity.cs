using System.Drawing;
using System.Numerics;
using PrettyApp.drawable;
using PrettyApp.util;
using PrettyApp.window;

namespace PrettyApp.lizard;
/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
public class LizardEntity : Entity
{
    private Vector2 _goal = new(100, 100); // the final destination the entity wants to reach
    private Vector2 _stepGoal = new(100, 100); // the eased-in step of the final goal
    private Vector2 _vel = new(7, 3);
    private float speed = 5.0f;


    internal LizardBody Body;
    internal SegmentLineEntity Tail;
    internal LizardLeg[] Legs;


    public LizardEntity(Point pos) : base(pos)
    {
        _vel = new(3 + (float)Random.Shared.NextDouble() * 5, 3 + (float)Random.Shared.NextDouble() * 5);

        App.Entities.Add(this);

        Body = new LizardBody(this);
        Tail = new LizardTail(this);

        Legs =
        [
            // order of legs matters!
            new LizardLeg(this, 1, -Util.PI / 2.5f),
            new LizardLeg(this, 6, -Util.PI / 2.5f),
            new LizardLeg(this, 6, Util.PI / 2.5f),
            new LizardLeg(this, 1, Util.PI / 2.5f),
        ];
    }


    protected override void RedrawPixelData()
    {
        AddRect((int)_goal.X, (int)_goal.Y, 1, 0xffffff);
    }

    public override void Tick()
    {
        _goal.X += _vel.X;
        _goal.Y += _vel.Y;

        const int sizeOffset = 0;

        if (_goal.X <= -sizeOffset || _goal.X >= MainWindow.bm.PixelWidth + sizeOffset)
            _vel.X *= -1;

        if (_goal.Y <= -sizeOffset || _goal.Y >= MainWindow.bm.PixelHeight + sizeOffset)
            _vel.Y *= -1;

        // move goal
        Pos.X = (int)_goal.X;
        Pos.Y = (int)_goal.Y;

        // update self rendering
        UpdateRequired = true;

        // allow most-stretched leg to step over
        float slowDownFactor = 1;
        int mostStretchedLegIndex = 0;

        for (int i = 0; i < Legs.Length; i++)
        {
            LizardLeg leg = Legs[i];
            leg.canMoveFoot = false;

            // find most stretched leg
            if (leg.currentStretch > Legs[mostStretchedLegIndex].currentStretch)
                mostStretchedLegIndex = i;

            if (leg.currentStretch > 1)
                slowDownFactor *= 0.8f;
        }

        // diagonal legs are allowed to step over
        Legs[mostStretchedLegIndex].canMoveFoot = true;
        Legs[(mostStretchedLegIndex + 2) % 4].canMoveFoot = true;


        // tell body where to move
        // speed is halted by high foot stretch
        CalculateStepGoal();
        Body.Goal = Body._segments[0].Pos + _stepGoal * slowDownFactor;
    }

    public override void TickSecond()
    {
    }

    private void CalculateStepGoal()
    {
        Vector2 stepV = new(Body._segments[0].Pos.X, Body._segments[0].Pos.Y);
        Vector2 goalDir = Vector2.Normalize(_goal - stepV); // direction to goal from head
        _stepGoal = Vector2.Normalize(goalDir) * speed;
    }
}