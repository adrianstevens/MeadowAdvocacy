using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThermalCamera
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        LedMatrix8x16Wing ledMatrix8X16Wing;

        Random random = new Random();

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            ledMatrix8X16Wing = new LedMatrix8x16Wing(Device.CreateI2cBus());

            graphics = new MicroGraphics(ledMatrix8X16Wing);
            graphics.CurrentFont = new Font4x8();

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            while (true)
            {
                RandomFill();
                RandomClear();
                CrossHairs();
            }
        }

        void Rotate()
        {
            bool on = true;

            graphics.Clear();

            for (int i = 0; i < 100; i++)
            {
                for(int x = 0; x < 7; x++) 
                { 
                }

                graphics.Show();

                Thread.Sleep(50);
            }
        }

        void RandomFill()
        {
            graphics.Clear();

            for(int i = 0; i < 100; i++) 
            {
                int x = random.Next() % 8;
                int y = random.Next() % 16;

                graphics.DrawPixel(x, y);

                graphics.Show();

                Thread.Sleep(50);
            }
        }

        void RandomClear()
        {
            for (int i = 0; i < 100; i++)
            {
                int x = random.Next() % 8;
                int y = random.Next() % 16;

                graphics.DrawPixel(x, y, false);

                graphics.Show();

                Thread.Sleep(50);
            }
        }

        void CrossHairs()
        {
            for (int i = 0; i < 16; i++)
            {
                graphics.Clear();

                graphics.DrawLine(0, i, 8, i);
                graphics.DrawLine(i / 2, 0, i / 2, 16);

                graphics.Show();

                Thread.Sleep(100);
            }
        }
    }
}