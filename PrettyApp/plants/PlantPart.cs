using PrettyApp.drawable;

namespace PrettyApp.plants;

public abstract class PlantPart(IPlantPart parent) : IPlantPart
{
    public bool ReadyToDestroy = false;
    protected bool UpdateRequired = true;

    protected int OffX = parent.GetX(), OffY = parent.GetY();
    public List<(int, int, int)> PixelDataList = new();
    public List<PlantPart> SubParts = new();

    protected IPlantPart Parent = parent;


    public List<(int, int, int)> PixelData
    {
        get
        {
            if (UpdateRequired)
            {
                UpdatePixelData();
                UpdateRequired = false;
            }

            return PixelDataList;
        }
        protected set => PixelDataList = value;
    }


    public void TickIncludingParts()
    {
        Tick();
        foreach (PlantPart plantPart in SubParts)
        {
            plantPart.TickIncludingParts();
        }
    }

    public abstract void Tick();
    protected abstract void UpdatePixelData();

    protected void AddPixel(int x, int y, int color)
    {
        PixelDataList.Add((x + OffX, y + OffY, color));
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
        int signX = Math.Sign(ex - sx);
        int sizeY = Math.Abs(ey - sy);
        int signY = Math.Sign(ey - sy);

        if (sizeX > sizeY)
        {
            for (int i = 0; i < sizeX; i++)
            {
                AddPixel(sx + i * signX, sy + (int)((ey - sy) * ((float)i / sizeX)) * signY, color);
            }
        }
        // else
        // {
        //     smaller = sy;
        //     larger = ey;
        //     
        //     if (sy < ey)
        //     {
        //         larger = sy;
        //         smaller = ey;
        //     }
        //     for (int i = sy; i <= ey; i++)
        //     {
        //         PixelDataList.Add((sy + (int)(Math.Abs(sy - ey) * ((float)i / ex)), i, color));
        //     }
        // }
    }

    public int GetX()
    {
        return OffX;
    }

    public int GetY()
    {
        return OffY;
    }

    public List<PlantPart> GetParts()
    {
        return SubParts;
    }

    public void TickIncludingPartsSecond()
    {
        TickSecond();
        foreach (PlantPart plantPart in SubParts)
        {
            plantPart.TickIncludingPartsSecond();
        }
    }

    protected abstract void TickSecond();
}