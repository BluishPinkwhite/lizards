namespace PrettyApp.drawable;

public class Util
{
    public static int LerpColor(int color1, int color2, float progress)
    {
        int r1 = color1 >> 16;
        int r2 = color2 >> 16;
        
        int colorData = ((int)((r1 - r2) * progress) + r2) << 16; // R
        r1 = (color1 & 0x00ff00) >> 8;
        r2 = (color2 & 0x00ff00) >> 8;
        
        colorData |= ((int)((r1 - r2) * progress) + r2) << 8;   // G
        r1 = color1 & 0x0000ff;
        r2 = color2 & 0x0000ff;
        
        colorData |= ((int)((r1 - r2) * progress) + r2);   // B
        return colorData;
    }

    
    public static int LerpInt(int i1, int i2, float progress)
    {
        return (int)((i1 - i2) * progress) + i1;
    }
}