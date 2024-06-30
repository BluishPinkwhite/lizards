using System.Numerics;

namespace PrettyApp.util;

public struct Segment
{
    public Vector2 Pos;
    public float Length;
    public float AngleFreedom; // 0-360

    public Segment(Vector2 pos, float length, float angleFreedom = 360f)
    {
        Pos = pos;
        Length = length;
        AngleFreedom = angleFreedom;
    }
}