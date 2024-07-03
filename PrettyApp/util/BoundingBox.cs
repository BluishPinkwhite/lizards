namespace PrettyApp.util;
/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
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

    public void ClampToScreen(int screenWidth, int screenHeight)
    {
        X = Math.Clamp(X, 0, screenWidth);
        Ex = Math.Clamp(Ex, 0, screenWidth);
        Y = Math.Clamp(Y, 0, screenHeight);
        Ey = Math.Clamp(Ey, 0, screenHeight);
    }
}