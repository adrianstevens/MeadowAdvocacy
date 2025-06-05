using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera;

class ArducamMini2MPPlus : ArducamBase
{
    public enum JpegResolution : byte
    {
        _160x120 = 0x00,
        _176x144 = 0x01,
        _320x240 = 0x02,
        _352x288 = 0x03,
        _640x480 = 0x04,
        _800x600 = 0x05,
        _1024x768 = 0x06,
        _1280x1024 = 0x07,
        _1600x1200 = 0x08
    }

    public ArducamMini2MPPlus(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte i2cAddress = (byte)Addresses.Default)
        : base(spiBus, chipSelectPin, i2cBus, i2cAddress)
    {
    }

    public override void ValidateCamera()
    {
        Console.WriteLine("ValidateCamera");

        while (true)
        {
            spiComms.WriteRegister(ARDUCHIP_TEST1, 0x55);
            var value = spiComms.ReadRegister(ARDUCHIP_TEST1);
            if (value == 0x55)
            {
                Console.WriteLine("Camera initialized");
                break;
            }
            Console.WriteLine($"Waiting for camera to initialize {value}");
            Thread.Sleep(1000);
        }

        while (true)
        {
            WriteRegisterI2C(0xff, 0x01);
            byte vid = ReadRegisterI2C(Ov2640Regs.OV2640_CHIPID_HIGH);
            byte pid = ReadRegisterI2C(Ov2640Regs.OV2640_CHIPID_LOW);

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
    }

    /// <summary>
    /// Init for OV2640 + Mini + Mini 2mp Plus
    /// </summary>
    public override void Initialize()
    {
        WriteRegisterI2C(0xff, 0x01);
        WriteRegisterI2C(0x12, 0x80);

        Thread.Sleep(100);

        if (format == ImageFormat.JPEG)
        {
            WriteRegistersI2C(Ov2640Regs.OV2640_JPEG_INIT);
            WriteRegistersI2C(Ov2640Regs.OV2640_YUV422);
            WriteRegistersI2C(Ov2640Regs.OV2640_JPEG);
            WriteRegisterI2C(0xff, 0x01);
            WriteRegisterI2C(0x15, 0x00);
            WriteRegistersI2C(Ov2640Regs.OV2640_320x240_JPEG); //leave this in place at 320x240
        }
        else
        {
            WriteRegistersI2C(Ov2640Regs.OV2640_QVGA);
        }
    }

    public void SetJpegResolution(JpegResolution resolution)
    {
        switch (resolution)
        {
            case JpegResolution._160x120:
                WriteRegistersI2C(Ov2640Regs.OV2640_160x120_JPEG);
                break;
            case JpegResolution._176x144:
                WriteRegistersI2C(Ov2640Regs.OV2640_176x144_JPEG);
                break;
            case JpegResolution._320x240:
                WriteRegistersI2C(Ov2640Regs.OV2640_320x240_JPEG);
                break;
            case JpegResolution._352x288:
                WriteRegistersI2C(Ov2640Regs.OV2640_352x288_JPEG);
                break;
            case JpegResolution._640x480:
                WriteRegistersI2C(Ov2640Regs.OV2640_640x480_JPEG);
                break;
            case JpegResolution._800x600:
                WriteRegistersI2C(Ov2640Regs.OV2640_800x600_JPEG);
                break;
            case JpegResolution._1024x768:
                WriteRegistersI2C(Ov2640Regs.OV2640_1024x768_JPEG);
                break;
            case JpegResolution._1280x1024:
                WriteRegistersI2C(Ov2640Regs.OV2640_1280x1024_JPEG);
                break;
            case JpegResolution._1600x1200:
                WriteRegistersI2C(Ov2640Regs.OV2640_1600x1200_JPEG);
                break;
            default:
                WriteRegistersI2C(Ov2640Regs.OV2640_320x240_JPEG);
                break;
        }

        Thread.Sleep(1000);

        FlushFifo();
        ClearFifoFlag();
    }

    public void SetLightMode(LightMode Light_Mode)
    {
        switch (Light_Mode)
        {
            case LightMode.Auto:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0xc7, 0x00); //AWB on
                break;
            case LightMode.Sunny:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0xc7, 0x40); //AWB off
                WriteRegisterI2C(0xcc, 0x5e);
                WriteRegisterI2C(0xcd, 0x41);
                WriteRegisterI2C(0xce, 0x54);
                break;
            case LightMode.Cloudy:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0xc7, 0x40); //AWB off
                WriteRegisterI2C(0xcc, 0x65);
                WriteRegisterI2C(0xcd, 0x41);
                WriteRegisterI2C(0xce, 0x4f);
                break;
            case LightMode.Office:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0xc7, 0x40); //AWB off
                WriteRegisterI2C(0xcc, 0x52);
                WriteRegisterI2C(0xcd, 0x41);
                WriteRegisterI2C(0xce, 0x66);
                break;
            case LightMode.Home:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0xc7, 0x40); //AWB off
                WriteRegisterI2C(0xcc, 0x42);
                WriteRegisterI2C(0xcd, 0x3f);
                WriteRegisterI2C(0xce, 0x71);
                break;
            default:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0xc7, 0x00); //AWB on
                break;
        }
    }

    private void SetColorSaturation(ColorSaturation saturation)
    {
        switch (saturation)
        {
            case ColorSaturation.Saturation2:

                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x02);
                WriteRegisterI2C(0x7c, 0x03);
                WriteRegisterI2C(0x7d, 0x68);
                WriteRegisterI2C(0x7d, 0x68);
                break;
            case ColorSaturation.Saturation1:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x02);
                WriteRegisterI2C(0x7c, 0x03);
                WriteRegisterI2C(0x7d, 0x58);
                WriteRegisterI2C(0x7d, 0x58);
                break;
            case ColorSaturation.Saturation0:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x02);
                WriteRegisterI2C(0x7c, 0x03);
                WriteRegisterI2C(0x7d, 0x48);
                WriteRegisterI2C(0x7d, 0x48);
                break;
            case ColorSaturation.Saturation_1:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x02);
                WriteRegisterI2C(0x7c, 0x03);
                WriteRegisterI2C(0x7d, 0x38);
                WriteRegisterI2C(0x7d, 0x38);
                break;
            case ColorSaturation.Saturation_2:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x02);
                WriteRegisterI2C(0x7c, 0x03);
                WriteRegisterI2C(0x7d, 0x28);
                WriteRegisterI2C(0x7d, 0x28);
                break;
        }
    }

    public void SetBrightness(Brightness brightness)
    {
        switch (brightness)
        {
            case Brightness.Brightness2:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x09);
                WriteRegisterI2C(0x7d, 0x40);
                WriteRegisterI2C(0x7d, 0x00);
                break;
            case Brightness.Brightness1:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x09);
                WriteRegisterI2C(0x7d, 0x30);
                WriteRegisterI2C(0x7d, 0x00);
                break;
            case Brightness.Brightness0:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x09);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x00);
                break;
            case Brightness.Brightness_1:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x09);
                WriteRegisterI2C(0x7d, 0x10);
                WriteRegisterI2C(0x7d, 0x00);
                break;
            case Brightness.Brightness_2:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x09);
                WriteRegisterI2C(0x7d, 0x00);
                WriteRegisterI2C(0x7d, 0x00);
                break;
        }
    }

    public void SetConstrast(Contrast contrast)
    {
        switch (contrast)
        {
            case Contrast.Contrast2:

                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x07);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x28);
                WriteRegisterI2C(0x7d, 0x0c);
                WriteRegisterI2C(0x7d, 0x06);
                break;
            case Contrast.Contrast1:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x07);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x24);
                WriteRegisterI2C(0x7d, 0x16);
                WriteRegisterI2C(0x7d, 0x06);
                break;
            case Contrast.Contrast0:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x07);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x06);
                break;
            case Contrast.Contrast_1:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x07);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x2a);
                WriteRegisterI2C(0x7d, 0x06);
                break;
            case Contrast.Contrast_2:
                WriteRegisterI2C(0xff, 0x00);
                WriteRegisterI2C(0x7c, 0x00);
                WriteRegisterI2C(0x7d, 0x04);
                WriteRegisterI2C(0x7c, 0x07);
                WriteRegisterI2C(0x7d, 0x20);
                WriteRegisterI2C(0x7d, 0x18);
                WriteRegisterI2C(0x7d, 0x34);
                WriteRegisterI2C(0x7d, 0x06);
                break;
        }
    }
}