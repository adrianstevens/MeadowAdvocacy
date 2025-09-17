using Graphics.MicroGraphics.Dither;
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Reflection;
using TideViewer;

namespace SilkDisplay_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static IFont fontLarge;
    static IFont fontMedium;
    static IFont fontSmall;

    static Image image;

    static IPixelBuffer ditheredBuffer;

    static Program()
    {
        _assemblyName = Assembly
             .GetExecutingAssembly()
             .GetName()
             .Name;
    }

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static async void Initialize()
    {




        fontLarge = new Font16x24();
        fontMedium = new Font12x20();
        fontSmall = new Font8x12();


        display = new SilkDisplay(displayScale: 1f);

        var virtualDisplay = new SimulatedEpd5in65f(rotate: true, displayRenderer: display);
        //var virtualDisplay = new SimulatedRm68140(rotate: true, displayRenderer: display, colorMode: ColorMode.Format12bppRgb444);

        image = GetImageResource("weather.bmp");

        var palette = new Color[]
        {
            Color.Black,
            Color.White,
            Color.Green,
            Color.Blue,
            Color.Red,
            Color.Yellow,
            Color.Orange
        };

        ditheredBuffer = PixelBufferDither.ToIndexed4(image.DisplayBuffer, palette, DitherMode.FloydSteinberg, true);

        graphics = new MicroGraphics(virtualDisplay)
        {
            CurrentFont = fontMedium,
            Stroke = 1,
        };

        // Set your Stormglass API key
        string apiKey = "8723c3ca-937d-11f0-b07a-0242ac130006-8723c456-937d-11f0-b07a-0242ac130006";

        // Coordinates for Entrance Island Lighthouse (north Gabriola Island)
        double lat = 49.208979;
        double lng = -123.809392;

        // Create service
        var tideService = new StormglassTideService(apiKey);

        // Time window: now → next 24h
        var start = DateTime.Now;
        var end = start.AddHours(24);

        try
        {
            // Fetch tide points
            List<TidePoint> points = await tideService.GetSeaLevelAsync(lat, lng, start, end);

            // Print simple table of times + heights
            Console.WriteLine($"Tides for Entrance Island {start:yyyy-MM-dd}");
            Console.WriteLine("Time (local)          Height (ft)");
            Console.WriteLine("-------------------  -----------");
            foreach (var p in points)
            {
                Console.WriteLine($"{p.Time:yyyy-MM-dd HH:mm}  {p.Level,8:0.00}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching tides: " + ex.Message);
        }


    }

    public static void Run()
    {
        int xCol1 = 40;
        int xCol2 = 180;

        Task.Run(() =>
        {
            while (true)
            {
                graphics.DrawBuffer(0, 0, ditheredBuffer);
                graphics.Show();

                /*
                graphics.Clear(Color.White);

                //graphics.DrawRectangle(4, 4, 120, 80, Color.Black, false);
                graphics.DrawCircle(54, 44, 37, Color.Orange, true);
                graphics.DrawCircle(54, 44, 37, Color.Black, false);

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
                */
            }
        });

        display!.Run();
    }

    private static readonly Dictionary<string, Image> _images = new();
    private static readonly string _assemblyName;
    private static Image GetImageResource(string name)
    {
        if (!_images.ContainsKey(name))
        {
            try
            {
                _images.Add(name, Image.LoadFromResource($"{_assemblyName}.{name}"));
            }
            catch
            {
                var availableResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                var match = availableResources.FirstOrDefault(r => r.Contains(name, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(match);
                    _images.Add(name, Image.LoadFromStream(stream));
                }
                else
                {
                    throw;
                }
            }
        }

        return _images[name];
    }
}

//<!=SNOP=>