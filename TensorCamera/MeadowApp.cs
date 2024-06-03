using BitMiracle.LibJpeg;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Peripherals.Displays;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LineChart
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        IProjectLabHardware projLab;

        Vc0706 camera;

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            camera = new Vc0706(Device, Device.PlatformOS.GetSerialPortName("COM1"), 38400);

            Console.WriteLine("Init complete");

            return base.Initialize();
        }

        public async override Task Run()
        {
            Console.WriteLine("Run...");

            for (int i = 0; i < 10; i++)
            {
                var picture = await TakePicture();

                graphics.Clear();

                Resolver.Log.Info("Draw picture");
                graphics.DrawBuffer(0, 0, picture);

                graphics.Show();

                Thread.Sleep(1000);
            }
        }

        public async Task<IPixelBuffer> TakePicture()
        {
            Resolver.Log.Info("Take a picture");
            camera.CapturePhoto();

            using var jpegStream = await camera.GetPhotoStream();
            var jpeg = new JpegImage(jpegStream);

            using var memoryStream = new MemoryStream();

            jpeg.WriteBitmap(memoryStream);
            byte[] bitmapData = memoryStream.ToArray();

            // Skip the first 54 bytes (bitmap header)
            byte[] pixelData = new byte[bitmapData.Length - 54];

            Resolver.Log.Info($"Bitmap data length: {bitmapData.Length}, Pixel data length: {pixelData.Length}");

            Array.Copy(bitmapData, 54, pixelData, 0, pixelData.Length);

            Resolver.Log.Info($"Width: {jpeg.Width}, Height: {jpeg.Height}");

            var pixelBuffer = new BufferRgb888(jpeg.Width, jpeg.Height, pixelData);

            return pixelBuffer.Resize<BufferGray8>(96, 96);
        }
    }
}