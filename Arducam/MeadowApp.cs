using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Sensors.Camera;
using SimpleJpegDecoder;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArducamMini
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        ArducamMini2MPPlus camera;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            camera = new ArducamMini2MPPlus(projLab.MikroBus1.SpiBus,
                projLab.MikroBus1.Pins.CS,
                projLab.MikroBus1.I2cBus);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            //may always need to be set, even if it matches the res in Initialize
            //  camera.SetJpegResolution(ArducamMini2MPPlus.JpegResolution._320x240);
            //  camera.ClearFifoFlag();

            for (int i = 0; i < 10; i++)
            {
                camera.StartCapture();
                Console.WriteLine("Start capture");

                while (camera.IsCaptureComplete() == false)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Capture not ready");
                }

                Console.WriteLine($"Capture {i} complete");
                Thread.Sleep(50);
                var jpegData = camera.ReadFifoBurst();
                camera.ClearFifoFlag();

                try
                {
                    if (jpegData.Length > 0)
                    {
                        var decoder = new JpegDecoder();
                        var jpg = decoder.DecodeJpeg(jpegData);
                        Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes, W: {decoder.Width}, H: {decoder.Height}");

                        var imageBuf = new BufferRgb888(decoder.Width, decoder.Height, jpg);

                        var graphics = new MicroGraphics(projLab.Display);

                        graphics.Clear();
                        graphics.DrawBuffer(0, 0, imageBuf);
                        graphics.Show();
                    }
                    else
                    {
                        Console.WriteLine($"Image capture failed {i}");
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to decode jpeg");
                }

                Thread.Sleep(4000);
            }

            return Task.CompletedTask;
        }

        async Task TakePicture()
        {
            camera.CapturePhoto();

            using var jpegStream = await camera.GetPhotoStream();

            Console.WriteLine($"Got photo stream: {jpegStream.Length}");

            //     var jpeg = new JpegImage(jpegStream);
            //     Resolver.Log.Info($"Image decoded - width:{jpeg.Width}, height:{jpeg.Height}");
        }
    }
}