using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.UI;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
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

    static PixelBufferBase image = default!;

    static Keyboard keyboard = default!;

    static TextDisplayMenu menu = default!;


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

        //image = LoadJpeg() as PixelBufferBase;

        var bmp = Image.LoadFromResource("lecs-logo.bmp");

        image = ((PixelBufferBase)bmp.DisplayBuffer).Convert<BufferRgb444>();

        Console.WriteLine("Load menu data...");

        var menuData = LoadResource("menu.json");

        Console.WriteLine($"Data length: {menuData.Length}...");

        Console.WriteLine("Create menu...");

        menu = new TextDisplayMenu(graphics, menuData, false);
        menu.ValueChanged += Menu_ValueChanged;
        Console.WriteLine("Create buttons...");

        next = GetPushButton(keyboard.Pins.Down);
        next.Clicked += (s, e) => { menu.Next(); };

        select = GetPushButton(keyboard.Pins.Right);
        select.Clicked += (s, e) => { menu.Select(); };

        previous = GetPushButton(keyboard.Pins.Up);
        previous.Clicked += (s, e) => { menu.Previous(); };

        back = GetPushButton(keyboard.Pins.Back);
        back.Clicked += (s, e) => { menu.Back(); };

        Console.WriteLine("Enable menu...");

        menu.Enable();
    }

    public static void Run()
    {
        Task.Run(() =>
        {
            // var grayImage = image.Convert<BufferGray8>();
            // var scaledImage = image.Resize<BufferGray8>(320, 320);
            // var rotatedImage = image.Rotate<BufferGray8>(new Meadow.Units.Angle(60));

            graphics.Clear();

            // draw the image centered
            // graphics.DrawBuffer((display!.Width - rotatedImage.Width) / 2, (display!.Height - rotatedImage.Height) / 2, rotatedImage);

            graphics.DrawBuffer(0, 0, image);

            var color = Color.Cyan;
            //   color = Color.FromHsba(180, 1, 0.18f);

            //color = Color.FromAhsv(1.0f, 180, 0.5f, 1.0f);

            color = Color.FromHsba(180, 1, 1);


            graphics.DrawRectangle(10, 10, 110, 110, color, true);

            graphics.Show();
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
        var resourceName = $"SilkDisplayTest.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}

//<!=SNOP=>