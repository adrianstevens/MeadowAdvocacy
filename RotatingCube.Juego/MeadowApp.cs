using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RotatingCubeJuego
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;
        MicroGraphics graphics;

        Cube3d cube;

        readonly int cubeSize = 60;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();
            graphics = new MicroGraphics(juego.Display);

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        public override Task Run()
        {
            cube = new Cube3d(graphics.Width / 2, graphics.Height / 2, cubeSize);

            cube.XVelocity = new Angle(2);
            cube.YVelocity = new Angle(3);
            cube.ZVelocity = new Angle(1);

            Color cubeColor = Color.Cyan;

            int frameCount = 0;
            string fpsText = "0.0fps";
            var sw = Stopwatch.StartNew();

            while (true)
            {
                graphics.Clear();

                cube.Update();

                DrawWireframe(cubeColor);

                graphics.DrawRectangle(2, 2, 80, 16, Color.Black, true);
                graphics.DrawText(2, 2, fpsText, Color.White);

                graphics.Show();

                cubeColor = cubeColor.WithHue(cubeColor.Hue + 0.001f);

                frameCount++;
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    float fps = frameCount * 1000f / sw.ElapsedMilliseconds;
                    fpsText = $"{fps:F1}fps";
                    Console.WriteLine(fpsText);
                    frameCount = 0;
                    sw.Restart();
                }
            }
        }

        void DrawWireframe(Color color)
        {
            var w = cube.Wireframe;

            // front face
            graphics.DrawLine(w[0, 0], w[0, 1], w[1, 0], w[1, 1], color);
            graphics.DrawLine(w[1, 0], w[1, 1], w[2, 0], w[2, 1], color);
            graphics.DrawLine(w[2, 0], w[2, 1], w[3, 0], w[3, 1], color);
            graphics.DrawLine(w[3, 0], w[3, 1], w[0, 0], w[0, 1], color);

            // front face cross
            graphics.DrawLine(w[1, 0], w[1, 1], w[3, 0], w[3, 1], color);
            graphics.DrawLine(w[0, 0], w[0, 1], w[2, 0], w[2, 1], color);

            // back face
            graphics.DrawLine(w[4, 0], w[4, 1], w[5, 0], w[5, 1], color);
            graphics.DrawLine(w[5, 0], w[5, 1], w[6, 0], w[6, 1], color);
            graphics.DrawLine(w[6, 0], w[6, 1], w[7, 0], w[7, 1], color);
            graphics.DrawLine(w[7, 0], w[7, 1], w[4, 0], w[4, 1], color);

            // connecting edges
            graphics.DrawLine(w[0, 0], w[0, 1], w[4, 0], w[4, 1], color);
            graphics.DrawLine(w[1, 0], w[1, 1], w[5, 0], w[5, 1], color);
            graphics.DrawLine(w[2, 0], w[2, 1], w[6, 0], w[6, 1], color);
            graphics.DrawLine(w[3, 0], w[3, 1], w[7, 0], w[7, 1], color);
        }
    }
}
