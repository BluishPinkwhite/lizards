using System.Drawing;
using System.Numerics;
using PrettyApp.drawable;
using PrettyApp.util;

namespace PrettyApp.lizard;

public class LizardEntity : Entity
{
    private Vector2 _goal = new(100, 100); // the final destination the entity wants to reach
    private Vector2 _stepGoal = new(100, 100); // the eased-in step of the final goal
    private Vector2 _vel = new(14, 6);
    private float speed = 8;
    
    
    private SegmentLineEntity Body;

    
    public LizardEntity(Point pos) : base(pos)
    {
        Segment[] bodySegments = new Segment[10];
        int len = 10;
        int angleFreedom = 5;
        int d = 0;
        
        for (int i = 0; i < bodySegments.Length; i++)
        {
            bodySegments[i] = new Segment(new Vector2((d++) * len, 0), len, angleFreedom);
        }
        Body = new SegmentLineEntity(new Point(0, 0), bodySegments);
        App.Entities.Add(Body);
    }


    protected override void RedrawPixelData()
    {
        AddRect((int)_goal.X, (int)_goal.Y, 1, 0);
        // AddRect((int)(_segments[0].Pos.X + _stepGoal.X), (int)(_segments[0].Pos.Y + _stepGoal.Y), 2, 0xff0000);
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
        

        UpdateRequired = true;
        
        CalculateStepGoal();
        Body.Goal = _stepGoal;
        
        
        
        Pos.X = (int)_goal.X;
        Pos.Y = (int)_goal.Y;
    }

    public override void TickSecond()
    {
        
    }
    
    private void CalculateStepGoal()
    {
        Vector2 stepV = new(Body._segments[0].Pos.X, Body._segments[0].Pos.Y);

        Vector2 goalDir = Vector2.Normalize(_goal - stepV); // direction to goal from head
        _stepGoal = stepV + Vector2.Normalize(goalDir) * speed;
    }
}