using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Camera;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Arcs
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        PersonSensor personSensor;

        MicroGraphics graphics;

        public override Task Run()
        {
            while (true)
            {
                var sensorData = personSensor.GetSensorData();

                graphics.Clear();


                for (int i = 0; i < sensorData.NumberOfFaces; ++i)
                {
                    var face = sensorData.FaceData[i];

                    if (face.BoxBottom > face.BoxTop)
                    {
                        graphics.DrawRectangle(face.BoxLeft / 2, face.BoxTop / 2, (face.BoxRight - face.BoxLeft) / 2, (face.BoxBottom - face.BoxTop) / 2, Color.Red, face.IsFacing == 1);
                    }
                    else
                    {
                        graphics.DrawRectangle(face.BoxLeft / 2, face.BoxBottom / 2, (face.BoxRight - face.BoxLeft) / 2, (face.BoxTop - face.BoxBottom) / 2, Color.Red, face.IsFacing == 1);
                    }


                    Console.WriteLine($"Face {i}: {face.BoxLeft}, {face.BoxBottom}, {face.BoxRight}, {face.BoxTop}, {face.IsFacing}");
                }

                graphics.DrawRectangle(0, 0, 128, 128, Color.White, false);

                graphics.Show();

                Thread.Sleep(1000);
            }


            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            personSensor = new PersonSensor(projLab.Qwiic.I2cBus);

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}