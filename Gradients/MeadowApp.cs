using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace Starfield
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);


            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();

            int size = 70;
            graphics.DrawHorizontalGradient(5, 5, size, size, Color.Yellow, Color.Purple);
            graphics.DrawHorizontalGradient(85, 5, size, size, Color.Orange, Color.Blue);
            graphics.DrawHorizontalGradient(165, 5, size, size, Color.Red, Color.Green);
            graphics.DrawHorizontalGradient(245, 5, size, size, Color.White, Color.Black);

            graphics.DrawVerticalGradient(5, 85, size, size, Color.Yellow, Color.LawnGreen);
            graphics.DrawVerticalGradient(85, 85, size, size, Color.LawnGreen, Color.Cyan);
            graphics.DrawVerticalGradient(165, 85, size, size, Color.Cyan, Color.Blue);
            graphics.DrawVerticalGradient(245, 85, size, size, Color.Blue, Color.Purple);

            graphics.DrawVerticalGradient(5, 165, size, size, Color.Purple, Color.Red);
            graphics.DrawVerticalGradient(85, 165, size, size, Color.Red, Color.Orange);
            graphics.DrawVerticalGradient(165, 165, size, size, Color.Orange, Color.Yellow);
            graphics.DrawVerticalGradient(245, 165, size, size, Color.Yellow, Color.Green);

            graphics.Show();

            return Task.CompletedTask;
        }
    }
}