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
            DoBackwardReaching(goalPoint, segments);
            DoForwardReaching(startPoint, segments);
        }
    }

    public static void DoBackwardReaching(Point goalPoint, Segment[] segments)
    {
        // map to V2
        Vector2 end = new Vector2(goalPoint.X, goalPoint.Y);

        const float tolerance = 0.8f; // Tolerance for endpoint fitting
        const int maxIterations = 20; // Allow more iterations for gradual convergence
        const float moveFraction = 0.5f; // Fraction of distance to move towards target in each iteration

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Check if the end is close enough to the goal
            if ((segments[^1].Pos - end).Length() < tolerance)
                break;

            // Backwards solve
            segments[^1].Pos =
                Vector2.Lerp(segments[^1].Pos, end, moveFraction); // Move last point slightly towards the goal
            for (int i = segments.Length - 1; i > 0; i--)
            {
                Vector2 dir = Vector2.Normalize(segments[i].Pos - segments[i - 1].Pos);

                if (i < segments.Length - 1)
                {
                    Vector2 nextDir = Vector2.Normalize(segments[i + 1].Pos - segments[i].Pos);
                    float dot = Vector2.Dot(dir, nextDir);
                    float angle = (float)Math.Acos(Math.Clamp(dot, -1f, 1f));

                    // Calculate signed angle
                    float cross = dir.X * nextDir.Y - dir.Y * nextDir.X; // 2D cross product to get the sign
                    float signedAngle = angle * Math.Sign(cross);

                    if (Math.Abs(signedAngle) > segments[i].AngleFreedom * Math.PI / 180)
                    {
                        float clampedAngleRad = Math.Sign(signedAngle) * segments[i].AngleFreedom * PI / 180;

                        // Smoothly rotate dir towards the clamped direction
                        dir = Vector2.Lerp(dir, new Vector2(
                            (float)(dir.X * Math.Cos(clampedAngleRad) - dir.Y * Math.Sin(clampedAngleRad)),
                            (float)(dir.X * Math.Sin(clampedAngleRad) + dir.Y * Math.Cos(clampedAngleRad))
                        ), moveFraction);

                        dir = Vector2.Normalize(dir); // Ensure dir is normalized after lerping
                    }
                }

                segments[i - 1].Pos = segments[i].Pos - dir * segments[i - 1].Length;
            }
        }
    }

    public static void DoForwardReaching(Point startPoint, Segment[] segments)
    {
        // map to V2
        Vector2 start = new Vector2(startPoint.X, startPoint.Y);

        const int maxIterations = 20; // Allow more iterations for gradual convergence
        const float moveFraction = 0.5f; // Fraction of distance to move towards target in each iteration

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Forwards solve
            segments[0].Pos =
                Vector2.Lerp(segments[0].Pos, start, moveFraction); // Move first point slightly towards the start
            for (int i = 0; i < segments.Length - 1; i++)
            {
                Vector2 dir = Vector2.Normalize(segments[i + 1].Pos - segments[i].Pos);

                if (i > 0)
                {
                    Vector2 prevDir = Vector2.Normalize(segments[i].Pos - segments[i - 1].Pos);
                    float dot = Vector2.Dot(prevDir, dir);
                    float angle = (float)Math.Acos(Math.Clamp(dot, -1f, 1f));

                    // Calculate signed angle
                    float cross = prevDir.X * dir.Y - prevDir.Y * dir.X; // 2D cross product to get the sign
                    float signedAngle = angle * Math.Sign(cross);

                    if (Math.Abs(signedAngle) > segments[i].AngleFreedom * Math.PI / 180)
                    {
                        float clampedAngleRad = Math.Sign(signedAngle) * segments[i].AngleFreedom * PI / 180;

                        // Smoothly rotate dir towards the clamped direction
                        dir = Vector2.Lerp(dir, new Vector2(
                            (float)(dir.X * Math.Cos(clampedAngleRad) - dir.Y * Math.Sin(clampedAngleRad)),
                            (float)(dir.X * Math.Sin(clampedAngleRad) + dir.Y * Math.Cos(clampedAngleRad))
                        ), moveFraction);

                        dir = Vector2.Normalize(dir); // Ensure dir is normalized after lerping
                    }
                }

                segments[i + 1].Pos = segments[i].Pos + dir * segments[i + 1].Length;
            }
        }
    }
}