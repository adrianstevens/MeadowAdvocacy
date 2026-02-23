using Boids.Core;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BoidsJuego
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;
        MicroGraphics graphics;
        BoidsEngine engine;

        // Rainbow LUT indexed by heading angle — 256 entries spanning full hue circle
        static readonly Color[] ColorLut = BuildColorLut();

        float _accelX;
        float _accelY;

        static Color[] BuildColorLut()
        {
            var lut = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                float h = i / 256f;
                lut[i] = HsvToColor(h, 0.85f, 1f);
            }
            return lut;
        }

        static Color HsvToColor(float h, float s, float v)
        {
            int hi = (int)(h * 6f) % 6;
            float f = h * 6f - MathF.Floor(h * 6f);
            float p = v * (1f - s);
            float q = v * (1f - f * s);
            float t = v * (1f - (1f - f) * s);
            return hi switch
            {
                0 => Color.FromRgb((byte)(v * 255), (byte)(t * 255), (byte)(p * 255)),
                1 => Color.FromRgb((byte)(q * 255), (byte)(v * 255), (byte)(p * 255)),
                2 => Color.FromRgb((byte)(p * 255), (byte)(v * 255), (byte)(t * 255)),
                3 => Color.FromRgb((byte)(p * 255), (byte)(q * 255), (byte)(v * 255)),
                4 => Color.FromRgb((byte)(t * 255), (byte)(p * 255), (byte)(v * 255)),
                _ => Color.FromRgb((byte)(v * 255), (byte)(p * 255), (byte)(q * 255)),
            };
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();
            graphics = new MicroGraphics(juego.Display);

            engine = new BoidsEngine();
            engine.Initialize();

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

            var sw = Stopwatch.StartNew();
            long lastMs = 0;

            int frameCount = 0;
            string fpsText = "";
            var fpsSw = Stopwatch.StartNew();

            while (true)
            {
                long nowMs = sw.ElapsedMilliseconds;
                float dt = (nowMs - lastMs) * 0.001f;
                if (dt > 0.05f) dt = 0.05f; // cap at 50ms to avoid spiral-of-death
                lastMs = nowMs;

                engine.SetWind(_accelX, _accelY);
                engine.Update(dt);

                graphics.Clear();
                DrawBoids();

                frameCount++;
                if (fpsSw.ElapsedMilliseconds >= 1000)
                {
                    fpsText = $"{frameCount} fps";
                    frameCount = 0;
                    fpsSw.Restart();
                }
                graphics.DrawText(2, 2, fpsText, Color.White);

                graphics.Show();
            }
        }

        void DrawBoids()
        {
            var x = engine.X;
            var y = engine.Y;
            var vx = engine.Vx;
            var vy = engine.Vy;
            int n = BoidsEngine.NumBoids;

            for (int i = 0; i < n; i++)
            {
                float px = x[i], py = y[i];
                float bvx = vx[i], bvy = vy[i];

                float speed = MathF.Sqrt(bvx * bvx + bvy * bvy);
                if (speed < 0.001f) continue;

                float inv = 1f / speed;
                float nx = bvx * inv; // unit forward
                float ny = bvy * inv;
                float rx = -ny;       // unit right (perpendicular)
                float ry =  nx;

                // Triangle: head at +5, base at -3 with ±3 width
                int hx = (int)(px + nx * 5f);
                int hy = (int)(py + ny * 5f);
                int lx = (int)(px - nx * 3f + rx * 3f);
                int ly = (int)(py - ny * 3f + ry * 3f);
                int rx2 = (int)(px - nx * 3f - rx * 3f);
                int ry2 = (int)(py - ny * 3f - ry * 3f);

                // Color by heading direction
                int colorIdx = (int)((MathF.Atan2(bvy, bvx) + MathF.PI) / (MathF.PI * 2f) * 256f) & 0xFF;
                Color c = ColorLut[colorIdx];

                graphics.DrawLine(hx, hy, lx, ly, c);
                graphics.DrawLine(hx, hy, rx2, ry2, c);
                graphics.DrawLine(lx, ly, rx2, ry2, c);
            }
        }
    }
}
