using Meadow.Hardware;

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

    public ArducamMini2MPPlus(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte i2cAddress)
        : base(spiBus, chipSelectPin, i2cBus, i2cAddress)
    { }

    public void SetJpegResolution(JpegResolution resolution)
    {
        switch (resolution)
        {
            case JpegResolution._160x120:
                WriteSensorRegisters(Ov2640Regs.OV2640_160x120_JPEG);
                break;
            case JpegResolution._176x144:
                WriteSensorRegisters(Ov2640Regs.OV2640_176x144_JPEG);
                break;
            case JpegResolution._320x240:
                WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG);
                break;
            case JpegResolution._352x288:
                WriteSensorRegisters(Ov2640Regs.OV2640_352x288_JPEG);
                break;
            case JpegResolution._640x480:
                WriteSensorRegisters(Ov2640Regs.OV2640_640x480_JPEG);
                break;
            case JpegResolution._800x600:
                WriteSensorRegisters(Ov2640Regs.OV2640_800x600_JPEG);
                break;
            case JpegResolution._1024x768:
                WriteSensorRegisters(Ov2640Regs.OV2640_1024x768_JPEG);
                break;
            case JpegResolution._1280x1024:
                WriteSensorRegisters(Ov2640Regs.OV2640_1280x1024_JPEG);
                break;
            case JpegResolution._1600x1200:
                WriteSensorRegisters(Ov2640Regs.OV2640_1600x1200_JPEG);
                break;
            default:
                WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG);
                break;
        }
    }

    public void SetLightMode(LightMode Light_Mode)
    {
        switch (Light_Mode)
        {
            case LightMode.Auto:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x00); //AWB on
                break;
            case LightMode.Sunny:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x5e);
                WriteSensorRegister(0xcd, 0x41);
                WriteSensorRegister(0xce, 0x54);
                break;
            case LightMode.Cloudy:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x65);
                WriteSensorRegister(0xcd, 0x41);
                WriteSensorRegister(0xce, 0x4f);
                break;
            case LightMode.Office:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x52);
                WriteSensorRegister(0xcd, 0x41);
                WriteSensorRegister(0xce, 0x66);
                break;
            case LightMode.Home:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x40); //AWB off
                WriteSensorRegister(0xcc, 0x42);
                WriteSensorRegister(0xcd, 0x3f);
                WriteSensorRegister(0xce, 0x71);
                break;
            default:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0xc7, 0x00); //AWB on
                break;
        }
    }

    private void SetColorSaturation(ColorSaturation saturation)
    {
        switch (saturation)
        {
            case ColorSaturation.Saturation2:

                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x68);
                WriteSensorRegister(0x7d, 0x68);
                break;
            case ColorSaturation.Saturation1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x58);
                WriteSensorRegister(0x7d, 0x58);
                break;
            case ColorSaturation.Saturation0:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x48);
                WriteSensorRegister(0x7d, 0x48);
                break;
            case ColorSaturation.Saturation_1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x38);
                WriteSensorRegister(0x7d, 0x38);
                break;
            case ColorSaturation.Saturation_2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x02);
                WriteSensorRegister(0x7c, 0x03);
                WriteSensorRegister(0x7d, 0x28);
                WriteSensorRegister(0x7d, 0x28);
                break;
        }
    }

    public void SetBrightness(Brightness brightness)
    {
        switch (brightness)
        {
            case Brightness.Brightness2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x40);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x30);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness0:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness_1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x10);
                WriteSensorRegister(0x7d, 0x00);
                break;
            case Brightness.Brightness_2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x09);
                WriteSensorRegister(0x7d, 0x00);
                WriteSensorRegister(0x7d, 0x00);
                break;
        }
    }

    public void SetConstrast(Contrast contrast)
    {
        switch (contrast)
        {
            case Contrast.Contrast2:

                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x28);
                WriteSensorRegister(0x7d, 0x0c);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x24);
                WriteSensorRegister(0x7d, 0x16);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast0:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast_1:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x2a);
                WriteSensorRegister(0x7d, 0x06);
                break;
            case Contrast.Contrast_2:
                WriteSensorRegister(0xff, 0x00);
                WriteSensorRegister(0x7c, 0x00);
                WriteSensorRegister(0x7d, 0x04);
                WriteSensorRegister(0x7c, 0x07);
                WriteSensorRegister(0x7d, 0x20);
                WriteSensorRegister(0x7d, 0x18);
                WriteSensorRegister(0x7d, 0x34);
                WriteSensorRegister(0x7d, 0x06);
                break;
        }
    }
}