using Graphics.MicroGraphics.Dither;
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Globalization;
using System.Reflection;
using TideViewer;

namespace SilkDisplay_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;
    static StormglassTideService tideService;

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

    public static void Initialize()
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
        string apiKey = "";



        // Create service
        tideService = new StormglassTideService(apiKey);

        /*
        // Coordinates for Entrance Island Lighthouse (north Gabriola Island)
        double lat = 49.208979;
        double lng = -123.809392;

        // Time window: now → next 24h
        var start = DateTime.Today;
        var end = start.AddHours(24);

        try
        {
            // Fetch tide points
            var points = await tideService.GetSeaLevelCachedAsync(lat, lng, start, end, preferEmbedded: true);



            //To refresh

            var fresh = await tideService.GetSeaLevelAsync(lat, lng, start, end);
            var cache = new TideCache
            {
                Lat = lat,
                Lng = lng,
                StartLocal = start,
                EndLocal = end,
                FetchedAtLocal = DateTime.Now,
                Points = fresh.ToArray()
            };

            string snippet = StormglassTideService.GenerateEmbeddedCacheSnippet(cache);


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
        */
    }

    public static void Run()
    {
        int xCol1 = 40;
        int xCol2 = 180;

        // Coordinates for Entrance Island Lighthouse (north Gabriola Island)
        double lat = 49.208979;
        double lng = -123.809392;

        // Time window: now → next 24h
        var start = DateTime.Now;
        var end = start.AddHours(24);

        var points = tideService.GetSeaLevelCached();

        _ = Task.Run(() =>
        {
            while (true)
            {
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

                //  graphics.DrawRectangle(274, 200, 322, 245, Color.Black, false);

                DrawTideGraph(points, 274, 200, 274 + 322, 200 + 245, "m");

                graphics.Show();

            }
        });

        display!.Run();
    }

    static void DrawTideGraph(IList<TidePoint> points, int x0, int y0, int x1, int y1, string yUnits = "m")
    {
        // normalize corners → box
        int left = Math.Min(x0, x1);
        int top = Math.Min(y0, y1);
        int right = Math.Max(x0, x1);
        int bottom = Math.Max(y0, y1);

        int W = right - left;
        int H = bottom - top;

        // frame (outer box)
        graphics.DrawHorizontalGradient(left, top, W, H, Color.SkyBlue, Color.White);


        graphics.DrawRectangle(left, top, right, bottom, Color.Black, false);

        // no data?
        if (points == null || points.Count < 2)
        {
            graphics.CurrentFont = new Font6x8();
            graphics.DrawText(left + 4, top + 4, "no tide data");
            return;
        }

        // scale
        double minV = points.Min(p => p.Level) - 0.1;
        double maxV = points.Max(p => p.Level) + 0.1;

        if (Math.Abs(maxV - minV) < 0.1)
        {
            maxV += 0.5;
            minV -= 0.5;
        }

        DateTime timeStart = points.First().Time;
        DateTime timeEnd = points.Last().Time;
        double totalMinutes = Math.Max(1, (timeEnd - timeStart).TotalMinutes);

        // y grid + labels
        graphics.CurrentFont = new Font6x8();
        int yGrids = 3;

        for (int i = 0; i <= yGrids; i++)
        {
            int y = top + (int)Math.Round(H * (1.0 - i / (double)yGrids));
            graphics.DrawLine(left, y, left + W, y, Color.Black);

            double v = minV + (maxV - minV) * (i / (double)yGrids);
            graphics.DrawText(left + 2, y - 9, v.ToString("0.0", CultureInfo.InvariantCulture) + yUnits, Color.Black);
        }

        // x ticks every 6h
        var step = TimeSpan.FromHours(6);
        var tick = RoundUp(timeStart, step);

        for (var t = tick; t <= timeEnd; t = t.Add(step))
        {
            int x = left + (int)Math.Round(W * ((t - timeStart).TotalMinutes / totalMinutes));
            graphics.DrawLine(x, top, x, top + H, Color.Black);
            if (t != tick)
            {
                graphics.DrawText(x - 2, bottom - 9, t.ToString("H:mm"), Color.Black, alignmentH: HorizontalAlignment.Right);
            }
        }

        // plot line
        int? lx = null, ly = null;

        foreach (var p in points)
        {
            int x = left + (int)Math.Round(W * ((p.Time - timeStart).TotalMinutes / totalMinutes));
            int y = top + (int)Math.Round(H * (1.0 - (p.Level - minV) / (maxV - minV)));

            if (lx.HasValue)
            {
                graphics.DrawLine(lx.Value, ly!.Value, x, y, Color.Blue);
            }

            lx = x; ly = y;
        }
    }

    static DateTime RoundUp(DateTime dt, TimeSpan step)
    {
        long ticks = ((dt.Ticks + step.Ticks - 1) / step.Ticks) * step.Ticks;
        return new DateTime(ticks, dt.Kind);
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