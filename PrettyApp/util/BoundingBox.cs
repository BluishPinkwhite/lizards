namespace PrettyApp.util;

public struct BoundingBox
{
    public int X, Y, Ex, Ey;

    public BoundingBox(int x, int y, int ex, int ey)
    {
        X = x;
        Y = y;
        Ex = ex;
        Ey = ey;
    }

    public BoundingBox()
    {
    }

    public int Width()
    {
        return Ex - X;
    }

    public int Height()
    {
        return Ey - Y;
    }
}