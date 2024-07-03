using System.Numerics;

namespace PrettyApp.util;
/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
public struct Segment
{
    public Vector2 Pos;
    public float Length;
    public float AngleFreedom; // 0-180

    public Segment(Vector2 pos, float length, float angleFreedom = 180f)
    {
        Pos = pos;
        Length = length;
        AngleFreedom = angleFreedom;
    }
}