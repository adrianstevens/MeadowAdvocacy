using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;

public class Helper 
{
    public static string romPath = "smb.nes";
    public static bool debugs0h = false;

    public static IPixelDisplay display;
    public static BufferRgb565 displayBuffer => display.PixelBuffer as BufferRgb565;
}