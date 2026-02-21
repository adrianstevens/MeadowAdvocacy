using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Mystify.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MystifyJuego
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;
        MicroGraphics graphics;
        MystifyEngine mystify;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            var shapes = mystify.Shapes;
            MystifyShape shape;
            Point[] points;
            int last;

            int frameCount = 0;
            float fps = 0;
            var sw = Stopwatch.StartNew();

            while (true)
            {
                graphics.Clear();

                for (int j = 0; j < shapes.Length; j++)
                {
                    shape = shapes[j];
                    points = shape.Points;
                    last = points.Length - 1;

                    for (int i = 0; i < last; i++)
                    {
                        graphics.DrawLine(
                            points[i].X, points[i].Y,
                            points[i + 1].X, points[i + 1].Y,
                            shape.Color);
                    }
                    graphics.DrawLine(
                        points[last].X, points[last].Y,
                        points[0].X, points[0].Y,
                        shape.Color);
                }

                frameCount++;
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    fps = frameCount * 1000f / sw.ElapsedMilliseconds;
                    frameCount = 0;
                    sw.Restart();
                }
                graphics.DrawText(2, 2, $"{fps:F1}fps", Color.White);

                graphics.Show();
                mystify.Update();
            }
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();
            graphics = new MicroGraphics(juego.Display);

            mystify = new MystifyEngine(juego.Display.Width, juego.Display.Height);
            mystify.NumberOfShapes = 3;
            mystify.PointsPerShape = 4;
            mystify.Initialize();

            return base.Initialize();
        }
    }
}
