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

        ArducamMini2MP camera;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            camera = new ArducamMini2MP(projLab.MikroBus2.SpiBus,
                projLab.MikroBus2.Pins.CS,
                projLab.MikroBus2.I2cBus);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            await camera.SetJpegSize(Arducam.ImageSize._352x288);

            var jpegData = await camera.CapturePhoto();


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

                Console.WriteLine("Complete");
            }
            else
            {
                Console.WriteLine("Image capture failed");
            }
        }
    }
}