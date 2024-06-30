using System.Drawing;
using System.Numerics;

namespace PrettyApp.util;

public class Util
{
    public static int LerpColor(int color1, int color2, float progress)
    {
        int r1 = color1 >> 16;
        int r2 = color2 >> 16;

        int colorData = ((int)((r1 - r2) * progress) + r2) << 16; // R
        r1 = (color1 & 0x00ff00) >> 8;
        r2 = (color2 & 0x00ff00) >> 8;

        colorData |= ((int)((r1 - r2) * progress) + r2) << 8; // G
        r1 = color1 & 0x0000ff;
        r2 = color2 & 0x0000ff;

        colorData |= ((int)((r1 - r2) * progress) + r2); // B
        return colorData;
    }


    public static int LerpInt(int i1, int i2, float progress)
    {
        return (int)((i1 - i2) * progress) + i1;
    }

    public static void DoFABRIK(Point startPoint, Point goalPoint, Segment[] segments)
    {
        // map to V2
        Vector2 start = new Vector2(startPoint.X, startPoint.Y);
        Vector2 end = new Vector2(goalPoint.X, goalPoint.Y);

        Vector2 direction = end - start;

        // is goal reachable?
        if (segments.Sum(s => s.Length) < direction.Length())
        {
            // unreachable -> straight line towards goal
            direction = Vector2.Normalize(direction);
            segments[0].Pos = start + direction * segments[0].Length;

            for (int i = 1; i < segments.Length; i++)
            {
                segments[i].Pos = segments[i - 1].Pos + direction * segments[i].Length;
            }
        }
        // reachable - do FABRIK
        // else
        // {
        //     for (int iteration = 0; iteration <= 3; iteration++)
        //     {
        //         // is close enough to goal?
        //         if ((segments[^1].Pos - end).Length() < 0.01f)
        //             break;
        //         
        //         // backwards solve
        //         segments[^1].Pos = end; // move last point to goal
        //         for (int i = segments.Length - 1; i > 0; i--)
        //         {
        //             segments[i-1].Pos = Vector2.Normalize(segments[i].Pos - segments[i - 1].Pos) * segments[i].Length;
        //         }
        //     }
        // }
    }
}