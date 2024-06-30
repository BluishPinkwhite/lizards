namespace PrettyApp.drawable;

public abstract class Entity : IPositionable
{
    protected int X, Y;
    protected List<(int, int, int)> PixelData { get; }
    protected bool UpdateRequired = true;


    public Entity(int x, int y, List<(int, int, int)> pixelData)
    {
        X = x;
        Y = y;
        PixelData = pixelData;
    }

    public Entity(int x, int y)
    {
        X = x;
        Y = y;
        PixelData = new();
    }

    public int GetX()
    {
        return X;
    }

    public int GetY()
    {
        return Y;
    }

    public List<(int, int, int)> GetPixelData()
    {
        return PixelData;
    }

    public void UpdatePixelData()
    {
        if (UpdateRequired)
        {
            UpdateRequired = false;
            PixelData.Clear();
            RedrawPixelData();
        }
    }

    protected abstract void RedrawPixelData();

    public abstract void Tick();
    public abstract void TickSecond();


    protected void AddPixel(int x, int y, int color)
    {
        PixelData.Add((x, y, color));
    }

    protected void AddRect(int sx, int ex, int sy, int ey, int color)
    {
        if (sx > ex) // swap start end, if start > end
        {
            (sx, ex) = (ex, sx);
        }

        if (sy > ey)
        {
            (sy, ey) = (ey, sy);
        }

        for (int i = sx; i <= ex; i++)
        {
            for (int j = sy; j <= ey; j++)
            {
                AddPixel(i, j, color);
            }
        }
    }

    protected void AddLine(int sx, int ex, int sy, int ey, int color)
    {
        int sizeX = Math.Abs(ex - sx);
        int sizeY = Math.Abs(ey - sy);

        if (sizeX > sizeY)
        {
            int signX = Math.Sign(ex - sx);
            for (int i = 0; i <= sizeX; i++)
            {
                AddPixel(sx + i * signX, sy + (int)((ey - sy) * ((float)i / sizeX)), color);
            }
        }
        else
        {
            int signY = Math.Sign(ey - sy);
            for (int i = 0; i <= sizeY; i++)
            {
                AddPixel(sx + (int)((ex - sx) * ((float)i / sizeY)), sy + i * signY, color);
            }
        }
    }
}