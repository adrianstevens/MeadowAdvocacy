using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Displays
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {

        MicroGraphics graphics;

        SharpMemoryDisplay display;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            graphics.Clear();

            graphics.DrawRectangle(0, 0, 80, 70, Color.White, true);

            graphics.DrawRectangle(40, 40, 120, 110, Color.Black, true);

            graphics.Show();

            Console.WriteLine("Run complete");

            return base.Run();
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            display = new SharpMemoryDisplay(
                Device.CreateSpiBus(new Units.Frequency(2000, Units.Frequency.UnitType.Kilohertz)),
                Device.Pins.D00);

            graphics = new MicroGraphics(display);


            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}