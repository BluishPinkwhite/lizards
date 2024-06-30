using System.Drawing;
using System.Numerics;

namespace PrettyApp.util;

public class Util
{
    public static readonly float PI = (float)Math.PI;
    
    
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
        else
        {
            for (int iteration = 0; iteration <= 5; iteration++)
            {
                // is close enough to goal? yes -> end
                if ((segments[^1].Pos - end).Length() < 0.1f)
                    break;

                // backwards solve
                segments[^1].Pos = new Vector2(end.X, end.Y); // move last point to goal
                for (int i = segments.Length - 1; i > 0; i--)
                {
                    Vector2 dir = Vector2.Normalize(segments[i].Pos - segments[i - 1].Pos);

                    if (i < segments.Length - 1)
                    {
                        Vector2 nextDir = Vector2.Normalize(segments[i + 1].Pos - segments[i].Pos);
                        float dot = Vector2.Dot(dir, nextDir);
                        float angle = (float)Math.Acos(dot) * 180f / PI;
                        
                        // ChatGPT: Calculate signed angle
                        float cross = dir.X * nextDir.Y - dir.Y * nextDir.X; // 2D cross product to get the sign
                        float signedAngle = angle * Math.Sign(cross);

                        if (Math.Abs(angle) > segments[i].AngleFreedom)
                        {
                            float clampedAngle = Math.Sign(signedAngle) * segments[i].AngleFreedom;
                            float clampedAngleRad = clampedAngle * PI / 180f;
                            
                            double sin = Math.Sin(clampedAngleRad);
                            double cos = Math.Cos(clampedAngleRad);

                            // Rotate dir by clamped angle
                            dir = new Vector2((float)(dir.X * cos - dir.Y * sin), (float)(dir.X * sin + dir.Y * cos));
                        }
                    }

                    segments[i - 1].Pos = segments[i].Pos - dir * segments[i].Length;
                }

                
                // forwards solve
                segments[0].Pos = new Vector2(start.X, start.Y);// +
                                  //Vector2.Normalize(segments[0].Pos - start) *
                                  //segments[0].Length; // move first point in front of start
                
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    Vector2 dir = Vector2.Normalize(segments[i + 1].Pos - segments[i].Pos);
                    
                    if (i > 0)
                    {
                        Vector2 prevDir = Vector2.Normalize(segments[i].Pos - segments[i - 1].Pos);
                        float dot = Vector2.Dot(prevDir, dir);
                        float angle = (float)Math.Acos(dot) * 180f / PI;
                        
                        // Calculate signed angle
                        float cross = prevDir.X * dir.Y - prevDir.Y * dir.X; // 2D cross product to get the sign
                        float signedAngle = angle * Math.Sign(cross);

                        if (Math.Abs(angle) > segments[i].AngleFreedom)
                        {
                            float clampedAngle = Math.Sign(signedAngle) * segments[i].AngleFreedom;
                            float clampedAngleRad = clampedAngle * PI / 180f;
                            
                            double sin = Math.Sin(clampedAngleRad);
                            double cos = Math.Cos(clampedAngleRad);

                            // Rotate dir by clamped angle
                            dir = new Vector2((float)(dir.X * cos - dir.Y * sin), (float)(dir.X * sin + dir.Y * cos));
                        }
                    }
                    
                    segments[i + 1].Pos = segments[i].Pos + dir * segments[i + 1].Length;
                }
            }
        }
    }
}