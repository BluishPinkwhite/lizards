namespace PrettyApp.util;


/**
 * Pixel data is stored in two integers (2x32bit int, 63-0):
 * 
 * 0-24 = RGB color (8 bits per channel)
 * 
 * 0-15 = pixel X position
 * 16-31 = pixel Y position
 */
public static class PixelDataComparer
{
    private sealed class PixelComparerEqualityComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y)
        {
            return x == y;
        }

        public int GetHashCode(int obj)
        {
            return obj;
        }
    }

    public static IEqualityComparer<int> PixelComparer { get; } = new PixelComparerEqualityComparer();

}