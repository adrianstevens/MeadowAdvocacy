using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Cameras;
using Meadow.Units;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera;

/// <summary>
/// Base class for Arducam family of cameras
/// </summary>
public abstract partial class ArducamBase : ICamera, ISpiPeripheral, II2cPeripheral
{
    //ToDo
    private byte imageFormat;

    protected virtual uint MAX_FIFO_SIZE => 0x5FFFF; //384KByte - OV2640 support


    /// <summary>
    /// The default SPI bus speed for the device
    /// </summary>
    public virtual Frequency DefaultSpiBusSpeed => new Frequency(8, Frequency.UnitType.Megahertz);

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

    internal ArducamBase(ISpiBus spiBus, IPin chipSelectPin, II2cBus i2cBus, byte i2cAddress)
        : this(spiBus, chipSelectPin.CreateDigitalOutputPort(), i2cBus, i2cAddress)
    { }

    internal ArducamBase(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, II2cBus i2cBus, byte i2cAddress)
    {
        i2cComms = new I2cCommunications(i2cBus, i2cAddress);
        spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        Resolver.Log.Info("Adrucam init...");

        //  Initialize();
    }

    /// <summary>
    /// Init for OV2640 + Mini + Mini 2mp Plus
    /// </summary>
    public void Initialize()
    {
        WriteSensorRegister(0xff, 0x01);
        WriteSensorRegister(0x12, 0x80);

        Thread.Sleep(100);

        if (imageFormat == JPEG)
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
    }

    public void FlushFifo()
    {
        WriteRegisterSPI(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    public void StartCapture()
    {
        WriteRegisterSPI(ARDUCHIP_FIFO, FIFO_START_MASK);
    }

    public void ClearFifoFlag()
    {
        WriteRegisterSPI(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
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
        len1 = ReadRegister(FIFO_SIZE1);
        len2 = ReadRegister(FIFO_SIZE2);
        len3 = (uint)(ReadRegister(FIFO_SIZE3) & 0x7f);
        length = ((len3 << 16) | (len2 << 8) | len1) & 0x07fffff;
        return length;
    }

    private void SetFifoBurst()
    {
        spiComms.Write(BURST_FIFO_READ);
    }

    private byte ReadFifo()
    {
        return BusReadSPI(SINGLE_FIFO_READ);
    }

    private void SetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        WriteRegisterSPI(address, (byte)(temp | bit));
    }

    private void ClearBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        WriteRegisterSPI(address, (byte)(temp & (~bit)));
    }

    public byte GetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        temp &= bit;
        return temp;
    }

    public byte ReadRegister(byte address)
    {
        return BusReadSPI(address);
    }

    private void SetMode(byte mode)
    {
        switch (mode)
        {
            case MCU2LCD_MODE:
                WriteRegisterSPI(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
            case CAM2LCD_MODE:
                WriteRegisterSPI(ARDUCHIP_MODE, CAM2LCD_MODE);
                break;
            case LCD2MCU_MODE:
                WriteRegisterSPI(ARDUCHIP_MODE, LCD2MCU_MODE);
                break;
            default:
                WriteRegisterSPI(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
        }
    }


    public void set_format(byte fmt)
    {
        if (fmt == BMP)
            imageFormat = BMP;
        else if (fmt == RAW)
            imageFormat = RAW;
        else
            imageFormat = JPEG;
    }




    public void WriteRegisterSPI(byte address, byte data)
    {
        BusWriteSPI(address, data);
    }

    private byte BusReadSPI(byte address)
    {
        return spiComms.ReadRegister((byte)(address & 0x7F));
    }

    private void BusWriteSPI(byte address, byte data)
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