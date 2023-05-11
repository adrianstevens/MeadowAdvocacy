using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThermalCamera
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        IProjectLabHardware projLab;

        Mlx90640 thermalCamera;

        MicroGraphics graphics;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            float[] frame;

            int pixelW = 7;
            int pixelH = 10;


            float min = 255;
            float max = 0;

            Color pixelColor;

            while (true)
            {
                frame = thermalCamera.ReadRawData();

                byte value;


                graphics.Clear();
                
                for (byte h = 0; h < 24; h++)
                {
                    for (byte w = 0; w < 32; w++)
                    {
                        value = (byte)((byte)frame[h * 32 + w] << 0);

                        min = Math.Min(min, value);
                        max = Math.Max(max, value);

                        pixelColor = new Color(value, value, 0);
                        graphics.DrawRectangle(8 + w * pixelW, h * pixelH, pixelW, pixelH, pixelColor, true);
                    }
                }

                graphics.Show();

                Thread.Sleep(500);

                Console.WriteLine($"tick {min} {max}");
            }

            return base.Run();
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            thermalCamera = new Mlx90640(projLab.I2cBus);

            IDigitalOutputPort chipSelectPort, resetPort;

            
            if (projLab is ProjectLabHardwareV2 { } projlabV2)
            {
        
            }
            else
            {
            
            }

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}