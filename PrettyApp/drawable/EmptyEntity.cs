namespace PrettyApp.drawable;

public class EmptyEntity : Entity
{
    public EmptyEntity(int x, int y, List<(int, int, int)> pixelData) : base(x, y, pixelData)
    {
    }

    public EmptyEntity(int x, int y) : base(x, y)
    {
    }

    protected override void RedrawPixelData()
    {
        // do nothing
    }

    public override void Tick()
    {
        // do nothing
    }

    public override void TickSecond()
    {
        // do nothing
    }
}