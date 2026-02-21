using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StarfieldJuego
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;
        MicroGraphics graphics;

        private const int NumberOfStars = 128;

        private Random random;

        private byte[] starX;
        private byte[] starY;
        private byte[] starZ;

        private Color[] colorLut;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            random = new Random();

            starX = new byte[NumberOfStars];
            starY = new byte[NumberOfStars];
            starZ = new byte[NumberOfStars];

            //pre-compute grayscale color lookup table
            colorLut = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                colorLut[i] = Color.FromRgb((byte)i, (byte)i, (byte)i);
            }

            juego = Juego.Create();
            graphics = new MicroGraphics(juego.Display);

            for (int i = 0; i < NumberOfStars; i++)
            {
                starX[i] = (byte)(160 - 120 + random.Next(256));
                starY[i] = (byte)random.Next(256);
                starZ[i] = (byte)(255 - i);
            }

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();

            int halfWidth = graphics.Width / 2;
            int halfHeight = graphics.Height / 2;
            int width = graphics.Width;
            int height = graphics.Height;

            int frameCount = 0;
            float fps = 0;
            string fpsText = "0.0fps";
            var sw = Stopwatch.StartNew();

            while (true)
            {
                for (int i = 0; i < NumberOfStars; i++)
                {
                    if (starZ[i] <= 1)
                    {
                        starX[i] = (byte)(halfWidth - halfHeight + random.Next(256));
                        starY[i] = (byte)random.Next(256);
                        starZ[i] = (byte)(255 - i);
                    }
                    else
                    {
                        int z = starZ[i];
                        int dx = starX[i] - halfWidth;
                        int dy = starY[i] - halfHeight;

                        int oldScreenX = dx * 256 / z + halfWidth;
                        int oldScreenY = dy * 256 / z + halfHeight;

                        graphics.DrawPixel(oldScreenX, oldScreenY, Color.Black);

                        z -= 2;
                        starZ[i] = (byte)z;

                        if (z > 1)
                        {
                            int screenX = dx * 256 / z + halfWidth;
                            int screenY = dy * 256 / z + halfHeight;

                            if (screenX >= 0 && screenY >= 0 &&
                                screenX < width && screenY < height)
                            {
                                graphics.DrawPixel(screenX, screenY, colorLut[255 - z]);
                            }
                            else
                            {
                                starZ[i] = 0;
                            }
                        }
                    }
                }

                graphics.DrawRectangle(2, 2, 80, 16, Color.Black, true);

                frameCount++;
                if (sw.ElapsedMilliseconds >= 1000)
                {
                    fps = frameCount * 1000f / sw.ElapsedMilliseconds;
                    frameCount = 0;
                    sw.Restart();
                    fpsText = $"{fps:F1}fps";
                }
                graphics.DrawText(2, 2, fpsText, Color.White);

                graphics.Show();
            }
        }
    }
}
