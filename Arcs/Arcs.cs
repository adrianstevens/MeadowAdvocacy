using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Arcs
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        Color arcColor = Color.Cyan;

        readonly int arcRadius = 60;

        public override Task Run()
        {
            int angle = 0;

            graphics.Stroke = 9;

            while (true)
            {
                graphics.Clear();

                graphics.DrawArc(120, 120, arcRadius, new Angle(angle += 5), new Angle(360), arcColor.WithHue(angle / 360.0), true);
                angle = angle % 360;

                graphics.Show();
            }
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}