using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using TravelClock.Core;

namespace TravelClock.Desktop;

public class Program
{
    static SilkDisplay display = default!;
    static MicroGraphics graphics = default!;
    static ClockController controller = default!;
    static Keyboard keyboard = default!;

    public static void Main()
    {
        Initialize();
        Run();
    }

    static void Initialize()
    {
        display = new SilkDisplay(320, 240, displayScale: 2f);

        graphics = new MicroGraphics(display)
        {
            Stroke = 1
        };

        controller = new ClockController(graphics);

        keyboard = new Keyboard();

        GetButton(keyboard.Pins.Right).Clicked += (s, e) => controller.NextView();
        GetButton(keyboard.Pins.Left).Clicked  += (s, e) => controller.PreviousView();
    }

    static void Run()
    {
        Task.Run(() => controller.Start());
        display.Run();
    }

    static IButton GetButton(IPin pin)
    {
        if (pin.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
            return new PushButton(pin, ResistorMode.InternalPullDown);
        else
            return new PollingPushButton(pin, ResistorMode.InternalPullDown);
    }
}
