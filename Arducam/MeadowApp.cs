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

            while (true)
            {
                camera.write_reg(Arducam.ARDUCHIP_TEST1, 0x55);
                var value = camera.read_reg(0x00);
                if (value == 0x55)
                {
                    Console.WriteLine("Camera initialized");
                    break;
                }
                Console.WriteLine("Waiting for camera");
                Thread.Sleep(1000);
            }

            while (true)
            {
                camera.wrSensorReg8_8(0xff, 0x01);
                byte vid = camera.rdSensorReg8_8(Ov2640Regs.OV2640_CHIPID_HIGH);
                byte pid = camera.rdSensorReg8_8(Ov2640Regs.OV2640_CHIPID_LOW);

                if ((vid != 0x26) && ((pid != 0x41) || (pid != 0x42)))
                {
                    Console.WriteLine($"Can't find OV2640 vid:{vid} pid:{pid}");
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("OV2640 detected");
                    break;
                }
            }

            camera.set_format((byte)Arducam.ImageFormat.Jpeg);
            camera.Initialize();
            Thread.Sleep(1000);
            camera.clear_fifo_flag();
            camera.write_reg(Arducam.ARDUCHIP_FRAMES, 0x00); //number of frames to capture

            //may always need to be set, even if it matches the res in Initialize
            camera.wrSensorRegs8_8(Ov2640Regs.OV2640_320x240_JPEG);
            Thread.Sleep(1000);

            camera.flush_fifo();
            camera.clear_fifo_flag();

            camera.start_capture();
            Console.WriteLine("Start capture");

            while (camera.get_bit(Arducam.ARDUCHIP_TRIG, Arducam.CAP_DONE_MASK) != 0)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Capture not ready");
            }

            Console.WriteLine("Capture complete");
            Thread.Sleep(50);
            var jpegData = camera.read_fifo_burst();
            camera.clear_fifo_flag();


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