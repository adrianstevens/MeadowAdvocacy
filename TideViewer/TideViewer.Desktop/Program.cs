using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace SilkDisplay_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static IFont fontLarge;
    static IFont fontMedium;
    static IFont fontSmall;

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        fontLarge = new Font16x24();
        fontMedium = new Font12x20();
        fontSmall = new Font8x12();


        display = new SilkDisplay(displayScale: 1f);

        var virtualDisplay = new SimulatedEpd5in65f(rotate: true, displayRenderer: display);

        graphics = new MicroGraphics(virtualDisplay)
        {
            CurrentFont = fontMedium,
            Stroke = 1,
        };
    }

    public static void Run()
    {
        int xCol1 = 40;
        int xCol2 = 180;

        Task.Run(() =>
        {
            while (true)
            {
                graphics.Clear(Color.White);

                graphics.DrawRectangle(4, 4, 120, 80, Color.Black, false);

                graphics.DrawText(150, 20, "22", Color.Black, ScaleFactor.X2, font: fontLarge);
                graphics.DrawText(220, 20, "°C", Color.Black, ScaleFactor.X1, font: fontMedium);
                graphics.DrawText(150, 70, "Feels like 80°", Color.Black, ScaleFactor.X1, font: fontSmall);

                graphics.DrawText(display!.Width - 4, 4, "Gabriola,BC", Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontLarge);
                graphics.DrawText(display!.Width - 4, 30, "Sunday,September 7", Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontMedium);

                graphics.DrawRectangle(4, 200, 30, 30, Color.Black, false);
                graphics.DrawText(xCol1, 200, "Sunrise", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 212, "7:17am", Color.Black, font: fontMedium);

                graphics.DrawRectangle(144, 200, 30, 30, Color.Black, false);
                graphics.DrawText(xCol2, 200, "Sunset", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 212, "7:17pm", Color.Black, font: fontMedium);

                graphics.DrawRectangle(4, 250, 30, 30, Color.Black, false);
                graphics.DrawText(xCol1, 250, "Wind", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 262, "12kn", Color.Black, font: fontMedium);

                graphics.DrawRectangle(144, 250, 30, 30, Color.Black, false);
                graphics.DrawText(xCol2, 250, "UV Index", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 262, "12", Color.Black, font: fontMedium);

                graphics.DrawRectangle(4, 300, 30, 30, Color.Black, false);
                graphics.DrawText(xCol1, 300, "Humidity", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 312, "80%", Color.Black, font: fontMedium);

                graphics.DrawRectangle(144, 300, 30, 30, Color.Black, false);
                graphics.DrawText(xCol2, 300, "Pressure", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 312, "1.01atm", Color.Black, font: fontMedium);

                graphics.DrawRectangle(4, 350, 30, 30, Color.Black, false);
                graphics.DrawText(xCol1, 350, "Air Quality", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 362, "30", Color.Black, font: fontMedium);

                graphics.DrawRectangle(144, 350, 30, 30, Color.Black, false);
                graphics.DrawText(xCol2, 350, "Visibility", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 362, "> 3km", Color.Black, font: fontMedium);

                graphics.DrawRectangle(274, 200, 322, 245, Color.Black, false);

                graphics.Show();
            }
        });

        display!.Run();
    }
}

//<!=SNOP=>