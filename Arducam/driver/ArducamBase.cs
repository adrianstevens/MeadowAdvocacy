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
    //ToDo change to an enum
    protected ImageFormat format;

    protected virtual int MAX_FIFO_SIZE => 0x5FFFF; //384KByte - OV2640 support


    /// <summary>
    /// The default SPI bus speed for the device
    /// </summary>
    public virtual Frequency DefaultSpiBusSpeed => new Frequency(2, Frequency.UnitType.Megahertz);

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
        Console.WriteLine("ArducamBase");

        i2cComms = new I2cCommunications(i2cBus, i2cAddress);
        spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        ResetCamera();

        Thread.Sleep(1000);

        ValidateCamera();

        SetImageFormat(ImageFormat.JPEG);
        Initialize();
        Thread.Sleep(1000);
        ClearFifoFlag();
        //spiComms.WriteRegister(ARDUCHIP_FRAMES, 0x00); .... not needed for 2MP plus???
    }

    protected void ResetCamera()
    {
        Console.WriteLine("ResetCamera");

        spiComms.WriteRegister(0x07, 0x80);
        Thread.Sleep(100);
        spiComms.WriteRegister(0x07, 0x00);
        Thread.Sleep(100);
    }

    public abstract void ValidateCamera();

    public abstract void Initialize();

    public bool IsCaptureComplete()
    {
        return GetBit(ARDUCHIP_TRIG, CAP_DONE_MASK) != 0;
    }

    public void FlushFifo()
    {
        spiComms.WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    public void StartCapture()
    {
        spiComms.WriteRegister(ARDUCHIP_FIFO, FIFO_START_MASK);
    }

    public void ClearFifoFlag()
    {
        spiComms.WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    public byte[] ReadFifoBurst()
    {
        int length = 0;

        while (length == 0)
        {
            length = ReadFifoLength();
            Console.WriteLine($"The fifo length is = {length}");
            Thread.Sleep(500);
        }
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
        tx[0] = BURST_FIFO_READ;
        var rx = new byte[length + 1];

        spiComms.Exchange(tx, rx, DuplexType.Full);

        int header = -1;
        int footer = -1;

        //search for jpeg header and footer
        for (int p = 0; p < rx.Length - 1; p++)
        {
            if (rx[p] == 0xFF && rx[p + 1] == 0xD8)
            {
                Console.WriteLine($"Found header {p}");
                header = p;
            }
            if (rx[p] == 0xFF && rx[p + 1] == 0xD9)
            {
                Console.WriteLine($"Found footer {p}");
                footer = p + 2;
                if (header != -1)
                {
                    break;
                }
            }
        }

        if (header == -1 || footer == -1)
        {
            Console.WriteLine("No image found");
            return new byte[0];
        }

        var image = new byte[footer - header];

        Array.Copy(rx, header, image, 0, footer - header);

        Console.WriteLine($"read_fifo_burst complete: {length}");
        return image;
    }

    private int ReadFifoLength()
    {
        byte len1, len2, len3;
        len1 = ReadRegister(FIFO_SIZE1);
        len2 = ReadRegister(FIFO_SIZE2);
        len3 = (byte)(ReadRegister(FIFO_SIZE3) & 0x7f);
        Console.WriteLine($"{len1}, {len2}, {len3}");
        var length = (len3 << 16) | (len2 << 8) | len1;
        return length;
    }

    private void SetFifoBurst()
    {
        spiComms.Write(BURST_FIFO_READ);
    }

    private byte ReadFifo()
    {
        return spiComms.ReadRegister(SINGLE_FIFO_READ);
    }

    private void SetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        spiComms.WriteRegister(address, (byte)(temp | bit));
    }

    private void ClearBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        spiComms.WriteRegister(address, (byte)(temp & (~bit)));
    }

    private byte GetBit(byte address, byte bit)
    {
        byte temp;
        temp = ReadRegister(address);
        temp &= bit;
        return temp;
    }

    protected byte ReadRegister(byte address)
    {
        return spiComms.ReadRegister(address);
    }

    private void SetMode(byte mode)
    {
        switch (mode)
        {
            case MCU2LCD_MODE:
                spiComms.WriteRegister(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
            case CAM2LCD_MODE:
                spiComms.WriteRegister(ARDUCHIP_MODE, CAM2LCD_MODE);
                break;
            case LCD2MCU_MODE:
                spiComms.WriteRegister(ARDUCHIP_MODE, LCD2MCU_MODE);
                break;
            default:
                spiComms.WriteRegister(ARDUCHIP_MODE, MCU2LCD_MODE);
                break;
        }
    }

    public void SetImageFormat(ImageFormat format)
    {
        this.format = format;
    }

    public byte ReadRegisterI2C(byte regID)
    {
        i2cComms.Write(regID);
        var ret = new byte[1];

        i2cComms.Read(ret);
        return ret[0];
    }
    public void WriteRegisterI2C(byte register, byte value)
    {
        i2cComms.WriteRegister(register, value);
    }

    // Write 8 bit values to 8 bit register regID
    public void WriteRegistersI2C(SensorReg[] reglist)
    {
        for (int i = 0; i < reglist.Length; i++)
        {
            WriteRegisterI2C(reglist[i].Register, reglist[i].Value);
        }
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