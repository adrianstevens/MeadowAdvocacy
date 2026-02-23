using FallingSand.Core;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FallingSandJuego
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;
        MicroGraphics graphics;
        FallingSandEngine engine;

        // 8 warm sandy shades from light tan to dark brown
        static readonly Color[] SandColors = new Color[]
        {
            Color.FromRgb(238, 214, 175),
            Color.FromRgb(224, 198, 152),
            Color.FromRgb(210, 182, 126),
            Color.FromRgb(196, 166, 104),
            Color.FromRgb(182, 150,  86),
            Color.FromRgb(168, 134,  70),
            Color.FromRgb(154, 118,  56),
            Color.FromRgb(140, 102,  44),
        };

        float _accelX;
        float _accelY;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();
            graphics = new MicroGraphics(juego.Display);

            engine = new FallingSandEngine();
            engine.Initialize(1500);

            if (juego.MotionSensor is { } bmi270)
            {
                bmi270.Updated += OnMotionUpdated;
                bmi270.StartUpdating(TimeSpan.FromMilliseconds(100));
            }

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        void OnMotionUpdated(object sender,
            IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Temperature? Temperature)> e)
        {
            if (e.New.Acceleration3D is { } accel)
            {
                _accelX = (float)accel.X.Gravity;
                _accelY = (float)accel.Y.Gravity;
            }
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            graphics.Clear();

            const int CellSize = 2;
            const int Cols = FallingSandEngine.Cols;
            const int Rows = FallingSandEngine.Rows;

            int frameCount = 0;
            string fpsText = "";
            var sw = Stopwatch.StartNew();

            while (true)
            {
                engine.SetGravity(_accelX, _accelY);
                engine.SpawnParticles(2);
                engine.Update();

                graphics.Clear();

                var grid = engine.Grid;
                var shade = engine.Shade;

                for (int y = 0; y < Rows; y++)
                {
                    int rowOffset = y * Cols;
                    int screenY = y * CellSize;
                    for (int x = 0; x < Cols; x++)
                    {
                        if (grid[rowOffset + x] == 1)
                        {
                            graphics.DrawRectangle(
                                x * CellSize, screenY,
                                CellSize, CellSize,
                                SandColors[shade[rowOffset + x]],
                                true);
                        }
                    }
                }

                frameCount++;
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    fpsText = $"{frameCount} fps";
                    frameCount = 0;
                    sw.Restart();
                }
                graphics.DrawText(2, 2, fpsText, Color.White);

                graphics.Show();
            }
        }
    }
}
