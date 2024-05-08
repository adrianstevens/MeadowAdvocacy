using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Mystify.Core;
using System;
using System.Threading.Tasks;

namespace MystifyProjectLab
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        MystifyEngine mystify;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            MystifyShape shape;

            while (true)
            {
                graphics.Clear();

                //draw the shapes
                for (int j = 0; j < mystify.Shapes.Length; j++)
                {
                    shape = mystify.Shapes[j];

                    for (int i = 0; i < shape.Points.Length; i++)
                    {
                        graphics.DrawLineAntialiased(
                                     shape.Points[i].X,
                                     shape.Points[i].Y,
                                     shape.Points[(i + 1) % shape.Points.Length].X,
                                     shape.Points[(i + 1) % shape.Points.Length].Y,
                                     shape.Color);
                    }
                }

                graphics.Show();

                mystify.Update();
            }
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            mystify = new MystifyEngine(projLab.Display.Width, projLab.Display.Height);

            mystify.NumberOfShapes = 3;
            mystify.PointsPerShape = 4;

            mystify.Initialize();


            return base.Initialize();
        }
    }
}