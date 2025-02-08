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

        private const int NumberOfStars = 128;

        private Random random;

        private byte[] starX;
        private byte[] starY;
        private byte[] starZ;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            random = new Random();

            starX = new byte[NumberOfStars];
            starY = new byte[NumberOfStars];
            starZ = new byte[NumberOfStars];

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

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

            while (true)
            {
                //  var startTime = DateTime.UtcNow.Ticks;

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
                        int oldScreenX = (starX[i] - halfWidth) * 256 / starZ[i] + halfWidth;
                        int oldScreenY = (starY[i] - halfHeight) * 256 / starZ[i] + halfHeight;

                        graphics.DrawPixel(oldScreenX, oldScreenY, Color.Black);

                        starZ[i] -= 2;
                        if (starZ[i] > 1)
                        {
                            int screenX = (starX[i] - halfWidth) * 256 / starZ[i] + halfWidth;
                            int screenY = (starY[i] - halfHeight) * 256 / starZ[i] + halfHeight;

                            if (screenX >= 0 && screenY >= 0 &&
                                screenX < graphics.Width && screenY < graphics.Height)
                            {
                                byte intensity = (byte)(255 - starZ[i]);
                                graphics.DrawPixel(screenX, screenY, Color.FromRgb(intensity, intensity, intensity));
                            }
                            else
                            {
                                starZ[i] = 0; // Out of screen, die.
                            }
                        }
                    }
                }

                graphics.Show();

                //    var endTime = DateTime.UtcNow.Ticks;
                //    double fps = 1.0 / ((endTime - startTime) / (double)TimeSpan.TicksPerSecond);
                //    Console.WriteLine($"FPS: {fps}");
            }
            return Task.CompletedTask;
        }
    }
}