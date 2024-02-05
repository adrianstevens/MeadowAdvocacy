using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Featherwings;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace SeeSawFeather
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        MiniTFTJoystickFeatherwingV1 wing = default!;

        MicroGraphics graphics = default!;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            graphics.Clear(Color.Red, true);
            graphics.DrawText(0, 0, "Hello!", Color.White);
            graphics.Show();

            Console.WriteLine("Run complete");


            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var config = new SpiClockConfiguration(new Meadow.Units.Frequency(12000, Meadow.Units.Frequency.UnitType.Kilohertz), SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(); // Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO,

            wing = new MiniTFTJoystickFeatherwingV1(Device, spiBus, Device.CreateI2cBus());

            graphics = new MicroGraphics(wing.Display)
            {
                CurrentFont = new Font12x16(),
            };

            /*
            wing.ButtonA.Clicked += (s, e) =>
            {
                Console.WriteLine("Button A clicked");
                graphics.Clear(Color.Azure, true);
            };

            wing.ButtonB.Clicked += (s, e) =>
            {
                Console.WriteLine("Button B clicked");
                graphics.Clear(Color.Blue, true);
            };

            wing.JoystickSelect.Clicked += (s, e) =>
            {
                Console.WriteLine("Button Select clicked");
                graphics.Clear(Color.SeaGreen, true);
            };
            */


            return Task.CompletedTask;
        }
    }
}