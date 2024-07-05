using System.Drawing;
using System.Numerics;
using PrettyApp.util;
using PrettyApp.window;

namespace PrettyApp.drawable;

/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
public abstract class Entity(Point pos)
{
    protected Point Pos = pos;
    private Dictionary<int, int> PixelData { get; } = new(PixelDataComparer.PixelComparer);
    protected BoundingBox Bounds, LastBounds;

    protected bool UpdateRequired = true;
    public bool HasJustUpdated = false;

    public Dictionary<int, int> GetPixelData()
    {
        return PixelData;
    }

    public void UpdatePixelData()
    {
        if (UpdateRequired)
        {
            UpdateRequired = false;
            HasJustUpdated = true;

            LastBounds = new BoundingBox(Bounds.X, Bounds.Y, Bounds.Ex, Bounds.Ey);
            Bounds = new(Pos.X, Pos.Y, Pos.X, Pos.Y);
            
            PixelData.Clear();
            RedrawPixelData();
        }
    }


    protected abstract void RedrawPixelData();

    public abstract void Tick();
    public abstract void TickSecond();


    protected void AddPixel(int x, int y, int color)
    {
        if (x < 0 || y < 0 || x >= MainWindow.bm.PixelWidth || y >= MainWindow.bm.PixelHeight)
            return;

        PixelData[(y << 16) + x] = color;

        
        if (x < Bounds.X)
            Bounds.X = x;
        else if (x > Bounds.Ex)
            Bounds.Ex = x;

        if (y < Bounds.Y)
            Bounds.Y = y;
        else if (y > Bounds.Ey)
            Bounds.Ey = y;
    }

    protected void AddPixel(Vector2 p, int color)
    {
        AddPixel((int)p.X, (int)p.Y, color);
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


    protected void AddRect(int x, int y, int size, int color)
    {
        AddRect(x - size, x + size, y - size, y + size, color);
    }


    protected void AddLine(Vector2 start, Vector2 end, int color)
    {
        AddLine((int)start.X, (int)end.X, (int)start.Y, (int)end.Y, color);
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

    public BoundingBox GetBoundingBox()
    {
        return Bounds;
    }

    public BoundingBox GetLastBoundingBox()
    {
        return LastBounds;
    }
}