using Graphics.MicroGraphics.Dither;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace TideViewer
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        MicroGraphics graphics;

        string assemblyName;

        IPixelBuffer background;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            assemblyName = Assembly
                 .GetExecutingAssembly()
                 .GetName()
                 .Name;

            var spiBus = Device.CreateSpiBus(new Meadow.Units.Frequency(48000, Meadow.Units.Frequency.UnitType.Kilohertz));

            var display = new Epd5in65f(spiBus, Device.Pins.A04, Device.Pins.A03, Device.Pins.A02, Device.Pins.A01);

            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font12x16(),
            };

            var image = GetImageResource("gallery.bmp");

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

            background = PixelBufferDither.ToIndexed4(image.DisplayBuffer, palette, DitherMode.FloydSteinberg, true);

            Console.WriteLine("Initialize complete");

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            graphics.Clear();
            graphics.DrawBuffer(0, 0, background);
            graphics.Show();

            return Task.CompletedTask;
        }

        private Image GetImageResource(string name)
        {
            //return Image.LoadFromResource($"{assemblyName}.{name}");
            return Image.LoadFromResource($"TideViewer.Meadow.{name}");
        }
    }
}