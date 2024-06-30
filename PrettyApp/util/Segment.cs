using System.Numerics;

namespace PrettyApp.util;

public struct Segment(Vector2 pos, float length)
{
    public Vector2 Pos = pos;
    public float Length = length;
}