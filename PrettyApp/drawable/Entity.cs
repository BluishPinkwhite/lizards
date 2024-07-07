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
            Bounds = new BoundingBox(Pos.X, Pos.Y, Pos.X, Pos.Y);

            PixelData.Clear();
            RedrawPixelData();
        }
    }


    protected abstract void RedrawPixelData();

    public abstract void Tick();
    public abstract void TickSecond();


    protected void AddPixel(int x, int y, int color)
    {
        if (x < 0)
        {
            
        }
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


    /**
     * Fills an area defined by 4 points in a loop.
     * First draws the outline with straight lines.
     * During that it saves min and max of X for each Y coordinate in bounds.
     * Then it goes row by row and fills in lines from minX to maxX.
     */
    protected void FillArea(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, int color)
    {
        int minY = Util.Min(p1.Y, p2.Y, p3.Y, p4.Y);
        int maxY = Util.Max(p1.Y, p2.Y, p3.Y, p4.Y);

        // array of "(min, max)"
        int[] minAndMaxX = new int[(maxY - minY + 1) * 2];
        for (int i = 0; i < minAndMaxX.Length; i++)
        {
            minAndMaxX[i] = -1;
        }

        AddLineWithWithWriteBack(p1, p2);
        AddLineWithWithWriteBack(p2, p3);
        AddLineWithWithWriteBack(p3, p4);
        AddLineWithWithWriteBack(p4, p1);


        void AddLineWithWithWriteBack(Vector2 a, Vector2 b)
        {
            int sizeX = (int)Math.Abs(b.X - a.X);
            int sizeY = (int)Math.Abs(b.Y - a.Y);

            if (sizeX > sizeY)
            {
                int signX = Math.Sign(b.X - a.X);
                for (int i = 0; i <= sizeX; i++)
                {
                    int xx = (int)(a.X + i * signX);
                    int yy = (int)(a.Y + (int)((b.Y - a.Y) * ((float)i / sizeX)));
                    AddPixel(xx, yy, color);

                    if (minAndMaxX[(yy - minY) * 2] == -1)
                    {
                        minAndMaxX[(yy - minY) * 2] = xx;
                        minAndMaxX[(yy - minY) * 2 + 1] = xx;
                    }
                    else
                    {
                        minAndMaxX[(yy - minY) * 2] = Math.Min(xx, minAndMaxX[(yy - minY) * 2]);
                        minAndMaxX[(yy - minY) * 2 + 1] = Math.Max(xx, minAndMaxX[(yy - minY) * 2 + 1]);
                    }
                }
            }
            else
            {
                int signY = Math.Sign(b.Y - a.Y);
                for (int i = 0; i <= sizeY; i++)
                {
                    int xx = (int)(a.X + (int)((b.X - a.X) * ((float)i / sizeY)));
                    int yy = (int)(a.Y + i * signY);
                    AddPixel(xx, yy, color);

                    if (minAndMaxX[(yy - minY) * 2] == -1)
                    {
                        minAndMaxX[(yy - minY) * 2] = xx;
                        minAndMaxX[(yy - minY) * 2 + 1] = xx;
                    }
                    else
                    {
                        minAndMaxX[(yy - minY) * 2] = Math.Min(xx, minAndMaxX[(yy - minY) * 2]);
                        minAndMaxX[(yy - minY) * 2 + 1] = Math.Max(xx, minAndMaxX[(yy - minY) * 2 + 1]);
                    }
                }
            }
        }

        for (int row = 0; row <= maxY - minY; row++)
        {
            for (int column = Math.Max(0, minAndMaxX[row * 2] - 1); 
                 column <= minAndMaxX[row * 2 + 1] + 1; column++)
            {
                if (column > 0)
                {
                    AddPixel(column, minY + row, color);
                }
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