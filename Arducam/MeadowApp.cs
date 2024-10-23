using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JuegoEyeball
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

        public override Task Run()
        {
            Console.WriteLine("Run...");

            camera.write_reg(0x07, 0x80);
            Thread.Sleep(100);
            camera.write_reg(0x07, 0x00);
            Thread.Sleep(100);

            Thread.Sleep(1000);
            camera.write_reg(0x00  /*ARDUCHIP_TEST1 */, 0x55);
            Thread.Sleep(5000);
            var value = camera.read_reg(0x00);

            Resolver.Log.Info($"temp: {value}");


            camera.write_reg(Arducam.ARDUCHIP_MODE, 0x00);

            camera.Initialize();

            camera.write_reg(Arducam.ARDUCHIP_MODE, 0x01);

            byte ARDUCHIP_TRIG = 0x41;
            byte VSYNC_MASK = 0x01;
            byte SHUTTER_MASK = 0x02;

            byte temp;

            while (true)
            {
                temp = camera.read_reg(ARDUCHIP_TRIG);

                if ((temp & VSYNC_MASK) != VSYNC_MASK)
                {
                    camera.write_reg(Arducam.ARDUCHIP_MODE, 0x00);
                    camera.write_reg(Arducam.ARDUCHIP_MODE, 0x01);

                    while ((camera.read_reg(ARDUCHIP_TRIG) & SHUTTER_MASK) != SHUTTER_MASK)
                    {
                        Console.WriteLine("Wait for camera");
                    }
                }

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