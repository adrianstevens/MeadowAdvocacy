using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System.Reflection;

namespace NESEmu;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    static Keyboard keyboard = default!;

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
        display = new SilkDisplay(320, 240, displayScale: 4f);

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font16x24(),
            Stroke = 1
        };

        keyboard = new Keyboard();

        Console.WriteLine("Load jpg...");

        Console.WriteLine("Create buttons...");

        next = GetPushButton(keyboard.Pins.Down);
        select = GetPushButton(keyboard.Pins.Right);
        previous = GetPushButton(keyboard.Pins.Up);
        back = GetPushButton(keyboard.Pins.Back);


        var layout = new StackLayout(0, 0, graphics.Width, graphics.Height, StackLayout.Orientation.Vertical);

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

    static byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"NESEmu.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}

//<!=SNOP=>