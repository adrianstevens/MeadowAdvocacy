using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using Meadow.Peripherals.Sensors.Buttons;
using System.Threading.Tasks;

namespace Skeeball;

internal class SkeeballHardware
{
    public IProjectLabHardware ProjLab { get; private set; }
    public Apa102 TopDisplay { get; private set; }
    public ILed[] Leds { get; private set; }

    public IButton StartButton { get; private set; }
    public IButton SelectButton { get; private set; }

    public Task Initialize()
    {
        ProjLab = ProjectLab.Create();

        StartButton = ProjLab.DownButton;
        SelectButton = ProjLab.UpButton;

        TopDisplay = new Apa102(ProjLab.MikroBus1.SpiBus, 32, 8);

        Leds = new ILed[5];
        Leds[0] = new Led(ProjLab.GroveAnalog.Pins.D0);
        Leds[1] = new Led(ProjLab.GroveAnalog.Pins.D1);
        Leds[2] = new Led(ProjLab.IOTerminal.Pins.A1);
        Leds[3] = new Led(ProjLab.IOTerminal.Pins.D2);
        Leds[4] = new Led(ProjLab.IOTerminal.Pins.D3);

        return Task.CompletedTask;
    }
}