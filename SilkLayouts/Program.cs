using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.UI;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using SimpleJpegDecoder;
using System.Reflection;

namespace SilkDisplay_Image_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static Keyboard keyboard = default!;

    static DisplayScreen screen;


    static IButton next = default!;
    static IButton previous = default!;
    static IButton select = default!;
    static IButton back = default!;

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        display = new SilkDisplay(640, 480, displayScale: 1f);

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font16x24(),
            Stroke = 1
        };

        keyboard = new Keyboard();

        Console.WriteLine("Load jpg...");

        var jpg = LoadJpeg() as PixelBufferBase;
        var image = Image.LoadFromPixelData(jpg);

        Console.WriteLine("Create buttons...");

        next = GetPushButton(keyboard.Pins.Down);
        select = GetPushButton(keyboard.Pins.Right);
        previous = GetPushButton(keyboard.Pins.Up);
        back = GetPushButton(keyboard.Pins.Back);


        var layout = new StackLayout(0, 0, graphics.Width, graphics.Height, StackLayout.Orientation.Vertical);

        var pic = new Picture(image.Width, image.Height, image);

        layout.Add(pic);

        layout.Add(new Label(100, 30, "Maple")
        {
            Font = new Font8x8(),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.LightGreen,
            BackgroundColor = Color.Blue
        });
        layout.Add(new Label(100, 30, "Trees")
        {
            Font = new Font8x12(),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.LawnGreen,
            BackgroundColor = Color.Blue
        });
        layout.Add(new Label(100, 30, "Are")
        {
            Font = new Font12x16(),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.Green,
            BackgroundColor = Color.Blue
        });
        layout.Add(new Label(100, 30, "Cool")
        {
            Font = new Font16x24(),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.DarkGreen,
            BackgroundColor = Color.Blue
        });

        screen = new DisplayScreen(display);
        screen.Controls.Add(layout);
    }

    public static void Run()
    {
        Task.Run(() =>
        {

        });

        display!.Run();
    }

    private static IButton GetPushButton(IPin pin)
    {
        if (pin.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
        {
            return new PushButton(pin, ResistorMode.InternalPullDown);
        }
        else
        {
            return new PollingPushButton(pin, ResistorMode.InternalPullDown);
        }
    }

    private static void Menu_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Console.WriteLine($"Value changed for {e.ItemID} to {e.Value}");
    }

    static IPixelBuffer LoadJpeg()
    {
        var jpgData = LoadResource("maple.jpg");

        var decoder = new JpegDecoder();
        var jpg = decoder.DecodeJpeg(jpgData);

        Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes, W: {decoder.Width}, H: {decoder.Height}");

        return new BufferRgb888(decoder.Width, decoder.Height, jpg);
    }

    static byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"SilkLayouts.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}

//<!=SNOP=>