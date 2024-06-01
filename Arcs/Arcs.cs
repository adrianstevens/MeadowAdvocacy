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

        readonly int arcRadius = 40;

        public override Task Run()
        {
            int start = -10;
            int end = 0;

            graphics.Stroke = 1;

            while (true)
            {
                graphics.Clear();

                end += 10;

                if (end <= 180)
                {
                    start += 5;
                }
                else
                {
                    start += 15;
                }
                start %= 360;
                end %= 360;

                graphics.DrawArc(120, 120, arcRadius, new Angle(start), new Angle(end), arcColor.WithHue(start / 360.0f), true);

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