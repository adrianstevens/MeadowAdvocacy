using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Sensors.Camera;
using SimpleJpegDecoder;
using System;
using System.Threading.Tasks;

namespace ArducamMini
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        Arducam camera;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            camera = new Arducam(projLab.MikroBus2.SpiBus,
                projLab.MikroBus2.Pins.CS,
                projLab.MikroBus2.I2cBus,
                (byte)Arducam.Addresses.Default);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            camera.Reset();

            await camera.Validate();

            await camera.Initialize();

            await camera.OV2640_SetJpegSize(Arducam.ImageSize._352x288);

            camera.Capture();

            var jpegData = camera.ReadFifoBurst();
            camera.ClearFifoFlag();

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
                Console.WriteLine("Image capture failed");
            }
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