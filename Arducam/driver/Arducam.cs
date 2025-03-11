using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Cameras;
using Meadow.Units;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera;

/// <summary>
/// Class that represents a Arducam family of cameras
/// </summary>
public partial class Arducam : ICamera, ISpiPeripheral, II2cPeripheral
{
    //ToDo
    private byte imageFormat;

    uint MAX_FIFO_SIZE = 0x5FFFF; //384KByte - OV2640 support


    /// <summary>
    /// The default SPI bus speed for the device
    /// </summary>
    public Frequency DefaultSpiBusSpeed => new Frequency(8, Frequency.UnitType.Megahertz);

    /// <summary>
    /// The SPI bus speed for the device
    /// </summary>
    public Frequency SpiBusSpeed
    {
        get => spiComms.BusSpeed;
        set => spiComms.BusSpeed = value;
    }

    /// <summary>
    /// The default SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <summary>
    /// The SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => spiComms.BusMode;
        set => spiComms.BusMode = value;
    }

    /// <summary>
    /// The default I2C bus for the camera
    /// </summary>
    public byte DefaultI2cAddress => 0x60;

    /// <summary>
    /// SPI Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly ISpiCommunications spiComms;

    /// <summary>
    /// I2C Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly II2cCommunications i2cComms;

    public Arducam(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte i2cAddress)
        : this(spiBus, chipSelectPin.CreateDigitalOutputPort(), i2cBus, i2cAddress)
    { }

    public Arducam(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, II2cBus i2cBus, byte i2cAddress)
    {
        i2cComms = new I2cCommunications(i2cBus, i2cAddress);
        spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        Resolver.Log.Info("Adrucam init...");
    }

    /// <summary>
    /// Init for OV2640 + Mini + Mini 2mp Plus
    /// </summary>
    public async Task Initialize()
    {
        SetImageFormat(ImageFormat.Jpeg);

        WriteSensorRegister(0xff, 0x01);
        WriteSensorRegister(0x12, 0x80);

        Thread.Sleep(100);

        if (imageFormat == (byte)ImageFormat.Jpeg)
        {
            WriteSensorRegisters(Ov2640Regs.OV2640_JPEG_INIT);
            WriteSensorRegisters(Ov2640Regs.OV2640_YUV422);
            WriteSensorRegisters(Ov2640Regs.OV2640_JPEG);
            WriteSensorRegister(0xff, 0x01);
            WriteSensorRegister(0x15, 0x00);
            WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG); //leave this in place at 320x240
        }
        else
        {
            WriteSensorRegisters(Ov2640Regs.OV2640_QVGA);
        }

        await Task.Delay(1000);
        ClearFifoFlag();
        WriteRegister(ARDUCHIP_FRAMES, 0x00); //number of frames to capture
    }

    public void Reset()
    {
        WriteRegister(0x07, 0x80);
        Thread.Sleep(100);
        WriteRegister(0x07, 0x00);
        Thread.Sleep(100);
    }

    public async Task Validate()
    {
        while (true)
        {
            WriteRegister(ARDUCHIP_TEST1, 0x55);
            var value = ReadRegsiter(0x00);
            if (value == 0x55)
            {
                Console.WriteLine("Camera initialized");
                break;
            }
            Console.WriteLine("Waiting for camera");
            await Task.Delay(1000);
        }

        while (true)
        {
            WriteSensorRegister(0xff, 0x01);
            byte vid = ReadSensorRegister(Ov2640Regs.OV2640_CHIPID_HIGH);
            byte pid = ReadSensorRegister(Ov2640Regs.OV2640_CHIPID_LOW);

            if ((vid != 0x26) && ((pid != 0x41) || (pid != 0x42)))
            {
                Console.WriteLine($"Can't find OV2640 vid:{vid} pid:{pid}");
                await Task.Delay(1000);
            }
            else
            {
                Console.WriteLine("OV2640 detected");
                break;
            }
        }
    }

    public void FlushFifo()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    public void SetCaptureResolution()
    {

    }

    public void Capture()
    {
        StartCapture();
        Console.WriteLine("Start capture");

        while (IsCaptureReady() == false)
        {
            Thread.Sleep(1000);
            Console.WriteLine("Capture not ready");
        }

        Console.WriteLine("Capture complete");
        Thread.Sleep(50);
    }

    public void StartCapture()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_START_MASK);
    }

    bool IsCaptureReady()
    {
        return GetBit(ARDUCHIP_TRIG, CAP_DONE_MASK) > 0;
    }

    public void ClearFifoFlag()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    public byte[] ReadFifoBurst()
    {
        uint length = ReadFifoLength();
        Console.WriteLine($"The fifo length is = {length}");

        if (length >= MAX_FIFO_SIZE)
        {
            length = MAX_FIFO_SIZE;
            Console.WriteLine("Fifo size (length) is over size.");
            return new byte[0];
        }
        if (length == 0)
        {
            Console.WriteLine("Size is 0");
            return new byte[0];
        }

        var tx = new byte[length + 1];
        tx[0] = 0x3C;
        var rx = new byte[length + 1];

        spiComms.Exchange(tx, rx, DuplexType.Full);

        int header = -1;
        int footer = -1;

        //search for jpeg header and footer
        for (int p = 0; p < rx.Length; p++)
        {
            if (rx[p] == 0xFF && rx[p + 1] == 0xD8)
            {
                Console.WriteLine($"Found header {p}");
                header = p;
            }
            if (rx[p] == 0xFF && rx[p + 1] == 0xD9)
            {
                Console.WriteLine($"Found footer {p}");
                footer = p;
                if (header != -1)
                {
                    break;
                }
            }
        }

        if (header == -1)
        {
            Console.WriteLine("No image found");
            return new byte[0];
        }
        if (footer == -1)
        {
            footer = (int)length;
        }
        else
        {
            footer += 2; //pad out to include footer bytes 
        }

        var image = new byte[footer - header];

        Array.Copy(rx, header, image, 0, footer - header);

        Console.WriteLine($"read_fifo_burst complete: {length}");
        return image;
    }

    private uint ReadFifoLength()
    {
        uint len1, len2, len3, length = 0;
        len1 = ReadRegsiter(FIFO_SIZE1);
        len2 = ReadRegsiter(FIFO_SIZE2);
        len3 = (uint)(ReadRegsiter(FIFO_SIZE3) & 0x7f);
        length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
        return length;
    }

    private void SetFifoBurst()
    {
        spiComms.Write(BURST_FIFO_READ);
    }

    private byte ReadFifo()
    {
        return BusReadSpi(SINGLE_FIFO_READ);
    }

    private void SetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegsiter(address);
        WriteRegister(address, (byte)(temp | bit));
    }

    private void ClearBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegsiter(address);
        WriteRegister(address, (byte)(temp & (~bit)));
    }

    public byte ReadRegsiter(byte address)
    {
        return BusReadSpi(address);
    }

    public byte GetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegsiter(address);
        temp &= bit;
        return temp;
    }

    private void SetMode(byte mode)
    {
        switch (mode)
        {
            case MCU2LCD_MODE:
                WriteRegister(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
            case CAM2LCD_MODE:
                WriteRegister(ARDUCHIP_MODE, CAM2LCD_MODE);
                break;
            case LCD2MCU_MODE:
                WriteRegister(ARDUCHIP_MODE, LCD2MCU_MODE);
                break;
            default:
                WriteRegister(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
        }
    }

    public async Task OV2640_SetJpegSize(ImageSize size)
    {
        switch (size)
        {
            case ImageSize._160x120:
                WriteSensorRegisters(Ov2640Regs.OV2640_160x120_JPEG);
                break;
            case ImageSize._176x144:
                WriteSensorRegisters(Ov2640Regs.OV2640_176x144_JPEG);
                break;
            case ImageSize._320x240:
                WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG);
                break;
            case ImageSize._352x288:
                WriteSensorRegisters(Ov2640Regs.OV2640_352x288_JPEG);
                break;
            case ImageSize._640x480:
                WriteSensorRegisters(Ov2640Regs.OV2640_640x480_JPEG);
                break;
            case ImageSize._800x600:
                WriteSensorRegisters(Ov2640Regs.OV2640_800x600_JPEG);
                break;
            case ImageSize._1024x768:
                WriteSensorRegisters(Ov2640Regs.OV2640_1024x768_JPEG);
                break;
            case ImageSize._1280x1024:
                WriteSensorRegisters(Ov2640Regs.OV2640_1280x1024_JPEG);
                break;
            case ImageSize._1600x1200:
                WriteSensorRegisters(Ov2640Regs.OV2640_1600x1200_JPEG);
                break;
            default:
                WriteSensorRegisters(Ov2640Regs.OV2640_320x240_JPEG);
                break;
        }
        await Task.Delay(1000);

        FlushFifo();
        ClearFifoFlag();
    }

    public void SetImageFormat(ImageFormat format)
    {
        imageFormat = (byte)format;
    }

    private void OV2640_SetLightMode(LightMode Light_Mode)
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

    //ToDo ... move to OV2640 specific class
    private void OV2640_set_Color_Saturation(ColorSaturation saturation)
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

    private void OV2640_set_Brightness(Brightness brightness)
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

    private void OV2640_set_Contrast(Contrast contrast)
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


    public void WriteRegister(byte address, byte data)
    {
        BusWrite(address, data);
    }

    private byte BusReadSpi(byte address)
    {
        return spiComms.ReadRegister((byte)(address & 0x7F));
    }

    private void BusWrite(byte address, byte data)
    {
        spiComms.WriteRegister((byte)(address | 0x80), data);
    }

    public byte ReadSensorRegister(byte regID)
    {
        i2cComms.Write(regID);
        var ret = new byte[1];

        i2cComms.Read(ret);
        return ret[0];
    }
    public int WriteSensorRegister(byte register, byte value)
    {
        i2cComms.WriteRegister(register, value);
        return 0;
    }

    // Write 8 bit values to 8 bit register regID
    public int WriteSensorRegisters(SensorReg[] reglist)
    {
        for (int i = 0; i < reglist.Length; i++)
        {
            WriteSensorRegister(reglist[i].Register, reglist[i].Value);
        }

        return 0;
    }

    public bool CapturePhoto()
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> GetPhotoData()
    {
        throw new NotImplementedException();
    }

    public Task<MemoryStream> GetPhotoStream()
    {
        throw new NotImplementedException();
    }

    public bool IsPhotoAvailable()
    {
        throw new NotImplementedException();
    }
}