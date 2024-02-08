using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace Arcs
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        public override Task Run()
        {
            DrawPolarLines();
            return base.Run();
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        void DrawPolarLines()
        {
            graphics.Clear();
            graphics.Stroke = 1;

            int dW = projLab.Display.Width;
            int dH = projLab.Display.Height;

            graphics.Stroke = 1;

            int colorIndex = 0;

            for (int i = 0; i < 270; i += 12)
            {
                graphics.DrawLine(dW / 2, dH / 2, (dW <= dH ? dW / 2 - 10 : dH / 2 - 10),
                    (float)(i * Math.PI / 180.0), Colors[colorIndex]);

                // increment our color index to go through all colors
                if (colorIndex < Colors.Length - 1)
                {
                    colorIndex++;
                }
                else
                {
                    colorIndex = 0;
                }
                graphics.Stroke++;
            }

            graphics.Show();
        }

        readonly Color[] Colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet };
    }
}