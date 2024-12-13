using Displays.ePaperWaveShare.Drivers;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace JuegoEyeball
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            var display = new Epd2in15g(
                    spiBus: projLab.MikroBus1.SpiBus,
                    chipSelectPin: projLab.MikroBus2.Pins.CS,
                    dcPin: projLab.MikroBus2.Pins.PWM,
                    resetPin: projLab.MikroBus2.Pins.RST,
                    busyPin: projLab.MikroBus2.Pins.INT);

            graphics = new MicroGraphics(display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run");

            graphics.Clear(Color.White);

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, 0, "Meadow F7", Color.Black, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 50, "Yellow", Color.Yellow, scaleFactor: ScaleFactor.X2);
            graphics.DrawText(0, 100, "Red", Color.Red, scaleFactor: ScaleFactor.X2);

            graphics.Show();

            Resolver.Log.Info("Run complete");

            return Task.CompletedTask;
        }
    }
}