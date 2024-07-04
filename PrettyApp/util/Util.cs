using System.Numerics;

namespace PrettyApp.util;

/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
public class Util
{
    public static readonly float PI = (float)Math.PI;
    private static readonly float LERP_AMOUNT = 0.5f;


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

    public static bool DoFABRIK(Vector2 start, Vector2 end, Vector2 moveDirection, Segment[] segments)
    {
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
            const float tolerance = 0.4f; // Tolerance for endpoint fitting
            const int maxIterations = 20; // Allow more iterations for gradual convergence

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Check if the end is close enough to the goal
                if ((segments[^1].Pos - end).Length() < tolerance)
                    break;

                DoBackwardReach(end, segments, false);
                DoForwardReach(start + moveDirection, segments, true);
            }
        }

        return segments.Sum(s => s.Length) < direction.Length();
    }

    public static void DoBackwardReaching(Vector2 end, Segment[] segments, bool teleportStart)
    {
        const int maxIterations = 20; // Allow more iterations for gradual convergence

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            DoBackwardReach(end, segments, teleportStart);
        }
    }

    private static void DoBackwardReach(Vector2 end, Segment[] segments, bool teleportStart)
    {
        // Backwards solve
        if (teleportStart)
        {
            segments[^1].Pos = end; // Move last point to the goal
        }

        else if ((end - segments[^1].Pos).Length() > 0)
        {
            Vector2 dir = Vector2.Normalize(end - segments[^1].Pos);
            Vector2 nextDir = Vector2.Normalize(segments[^1].Pos - segments[^2].Pos);

            float dot = Vector2.Dot(dir, nextDir);
            float angle = (float)Math.Acos(Math.Clamp(dot, -1f, 1f));

            // Calculate signed angle
            float cross = dir.X * nextDir.Y - dir.Y * nextDir.X; // 2D cross product to get the sign
            float signedAngle = angle * Math.Sign(cross);
            if (!float.IsNaN(cross))
            {
                if (Math.Abs(signedAngle) > segments[^1].AngleFreedom * PI / 180)
                {
                    float clampedAngleRad = -Math.Sign(signedAngle) * segments[^1].AngleFreedom * PI / 180;

                    // Smoothly rotate dir towards the clamped direction
                    dir = new Vector2(
                        (float)(dir.X * Math.Cos(clampedAngleRad) - dir.Y * Math.Sin(clampedAngleRad)),
                        (float)(dir.X * Math.Sin(clampedAngleRad) + dir.Y * Math.Cos(clampedAngleRad)));

                    dir = Vector2.Normalize(dir); // Ensure dir is normalized after lerping
                }

                segments[^1].Pos = Vector2.Lerp(segments[^1].Pos,
                    segments[^1].Pos + dir * (end - segments[^1].Pos).Length(),
                    LERP_AMOUNT);
            }
            else return;
        }
        else return;


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
                if (float.IsNaN(cross))
                    continue;

                float signedAngle = angle * Math.Sign(cross);

                if (Math.Abs(signedAngle) > segments[i].AngleFreedom * PI / 180)
                {
                    float clampedAngleRad = -Math.Sign(signedAngle) * segments[i].AngleFreedom * PI / 180;

                    // Smoothly rotate dir towards the clamped direction
                    dir = new Vector2(
                        (float)(dir.X * Math.Cos(clampedAngleRad) - dir.Y * Math.Sin(clampedAngleRad)),
                        (float)(dir.X * Math.Sin(clampedAngleRad) + dir.Y * Math.Cos(clampedAngleRad)));

                    dir = Vector2.Normalize(dir); // Ensure dir is normalized after lerping
                }
            }

            segments[i - 1].Pos = Vector2.Lerp(segments[i - 1].Pos,
                segments[i].Pos - dir * segments[i - 1].Length,
                LERP_AMOUNT);
        }
    }

    public static void DoForwardReaching(Vector2 start, Segment[] segments, bool teleportStart)
    {
        const int maxIterations = 20; // Allow more iterations for gradual convergence

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            if ((start - segments[0].Pos).Length() > 0.01f)
            {
                DoForwardReach(start, segments, teleportStart);
            }
        }
    }

    private static void DoForwardReach(Vector2 start, Segment[] segments, bool teleportStart)
    {
        // Forwards solve

        // Move first point slightly towards the start
        if (teleportStart)
        {
            segments[0].Pos = Vector2.Lerp(segments[0].Pos, start, LERP_AMOUNT);
        }

        else if ((start - segments[0].Pos).Length() > 0.01f)
        {
            Vector2 dir = Vector2.Normalize(start - segments[0].Pos);
            Vector2 prevDir = Vector2.Normalize(segments[0].Pos - segments[1].Pos);

            float dot = Vector2.Dot(prevDir, dir);
            float angle = (float)Math.Acos(Math.Clamp(dot, -1f, 1f));

            // Calculate signed angle
            float cross = prevDir.X * dir.Y - prevDir.Y * dir.X; // 2D cross product to get the sign
            if (!float.IsNaN(cross))
            {
                float signedAngle = angle * Math.Sign(cross);

                if (Math.Abs(signedAngle) > segments[0].AngleFreedom * PI / 180)
                {
                    float clampedAngleRad = -Math.Sign(signedAngle) * segments[0].AngleFreedom * PI / 180;

                    dir = new Vector2(
                        (float)(dir.X * Math.Cos(clampedAngleRad) - dir.Y * Math.Sin(clampedAngleRad)),
                        (float)(dir.X * Math.Sin(clampedAngleRad) + dir.Y * Math.Cos(clampedAngleRad)));

                    dir = Vector2.Normalize(dir); // Ensure dir is normalized after lerping
                }

                Vector2 temp = new Vector2(segments[0].Pos.X, segments[0].Pos.Y);
                float dist = (segments[0].Pos - start).Length();

                segments[0].Pos = Vector2.Lerp(segments[0].Pos,
                    segments[0].Pos + dir * (segments[0].Pos - start).Length(),
                    LERP_AMOUNT);

                // fix for wrong dir (segments flying off to infinity): flip dir when segment got farther by step
                if ((segments[0].Pos - start).Length() > dist)
                {
                    segments[0].Pos = Vector2.Lerp(temp,
                        temp - dir * (temp - start).Length(),
                        LERP_AMOUNT);
                }
            }
            else return;
        }
        else return;

        // Iterate through each segment to adjust positions
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
                if (float.IsNaN(cross))
                    continue;

                float signedAngle = angle * Math.Sign(cross);

                if (Math.Abs(signedAngle) > segments[i].AngleFreedom * PI / 180)
                {
                    float clampedAngleRad = -Math.Sign(signedAngle) * segments[i].AngleFreedom * PI / 180;

                    // Smoothly rotate dir towards the clamped direction
                    dir = new Vector2(
                        (float)(dir.X * Math.Cos(clampedAngleRad) - dir.Y * Math.Sin(clampedAngleRad)),
                        (float)(dir.X * Math.Sin(clampedAngleRad) + dir.Y * Math.Cos(clampedAngleRad)));

                    dir = Vector2.Normalize(dir); // Ensure dir is normalized after rotation
                }
            }

            segments[i + 1].Pos = Vector2.Lerp(segments[i + 1].Pos,
                segments[i].Pos + dir * segments[i + 1].Length,
                LERP_AMOUNT);
        }
    }

    public static Vector2 RotateVector(Vector2 vec, float angleRad)
    {
        float cos = (float)Math.Cos(angleRad);
        float sin = (float)Math.Sin(angleRad);

        return Vector2.Normalize(
            new Vector2(
                vec.X * cos - vec.Y * sin,
                vec.X * sin + vec.Y * cos));
    }
}