using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Cameras;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Camera;

/// <summary>
/// Class that represents a Arducam family of cameras
/// </summary>
public abstract partial class Arducam : IPhotoCamera, ISpiPeripheral, II2cPeripheral
{
    // make public when support for BMP and RAW are added
    protected ImageFormat CurrentImageFormat { get; set; } = ImageFormat.Jpeg;

    protected virtual uint MAX_FIFO_SIZE => 0x5FFFF; //384KByte - OV2640 support

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
    public byte DefaultI2cAddress => 0x30;

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

        Reset();

        Validate().Wait();
        Initialize().Wait();
    }

    public abstract Task Initialize();

    public void Reset()
    {
        WriteRegister(0x07, 0x80);
        Thread.Sleep(100);
        WriteRegister(0x07, 0x00);
        Thread.Sleep(100);
    }

    protected abstract Task Validate();

    public void FlushFifo()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    public async Task<byte[]> CapturePhoto()
    {
        StartCapture();

        bool readData = false;
        for (int i = 0; i < 10; i++)
        {
            if (IsPhotoAvailable() == false)
            {
                await Task.Delay(500);
            }
            else
            {
                readData = true;
                break;
            }
        }

        if (readData == false)
        {
            throw new Exception("Photo data not available");
        }

        await Task.Delay(50);


        return await GetPhotoData();
    }

    private void StartCapture()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_START_MASK);
    }

    private bool IsPhotoAvailable()
    {
        return GetBit(ARDUCHIP_TRIG, CAP_DONE_MASK) > 0;
    }

    protected void ClearFifoFlag()
    {
        WriteRegister(ARDUCHIP_FIFO, FIFO_CLEAR_MASK);
    }

    private Task<byte[]> GetPhotoData()
    {
        uint length = ReadFifoLength();
        Console.WriteLine($"The fifo length is = {length}");

        if (length >= MAX_FIFO_SIZE)
        {
            length = MAX_FIFO_SIZE;
            Console.WriteLine("Fifo size (length) is over size.");
            return Task.FromResult(new byte[0]);
        }
        if (length == 0)
        {
            Console.WriteLine("Size is 0");
            return Task.FromResult(new byte[0]);
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
            return Task.FromResult(new byte[0]);
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

        ClearFifoFlag();
        return Task.FromResult(image);
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

    protected byte ReadRegsiter(byte address)
    {
        return BusReadSpi(address);
    }

    protected byte GetBit(byte address, byte bit)
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

    public abstract Task SetJpegSize(ImageSize size);

    public void SetImageFormat(ImageFormat format)
    {
        CurrentImageFormat = format;
    }

    protected void WriteRegister(byte address, byte data)
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

    protected byte ReadSensorRegister(byte regID)
    {
        i2cComms.Write(regID);
        var ret = new byte[1];

        i2cComms.Read(ret);
        return ret[0];
    }

    protected int WriteSensorRegister(byte register, byte value)
    {
        i2cComms.WriteRegister(register, value);
        return 0;
    }

    // Write 8 bit values to 8 bit register regID
    protected internal int WriteSensorRegisters(SensorReg[] reglist)
    {
        for (int i = 0; i < reglist.Length; i++)
        {
            WriteSensorRegister(reglist[i].Register, reglist[i].Value);
        }

        return 0;
    }
}