using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThermalCamera
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        LedMatrix8x16Wing ledMatrix8X16Wing;

        MicroGraphics graphics;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            bool on = true;

            while (true)
            {
                graphics.Clear(on = !on);
                graphics.Show();

                Thread.Sleep(1000);
            }

        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            ledMatrix8X16Wing = new LedMatrix8x16Wing(Device.CreateI2cBus());

            graphics = new MicroGraphics(ledMatrix8X16Wing);
            graphics.CurrentFont = new Font4x8();

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}