namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Valid addresses for the SAMD09 Seesaw
/// </summary>
public enum BoardAddresses : byte
{
    /// <summary>
    /// I2C address 0x5E
    /// </summary>
    Address_0x5E = 0x5E,
    /// <summary>
    /// Default bus address
    /// </summary>
    Default = Address_0x5E,
}

enum HardwareId : byte
{
    ATSAMD09 = 0x55,
}

enum BaseAddresses : byte
{
    Status = 0x00,
    GPIO = 0x01,
    Sercom0 = 0x02,

    Timer = 0x08,
    ADC = 0x09,
    DAC = 0x0A,
    Interrupt = 0x0B,
    Dap = 0x0C,
    Eeprom = 0x0D,
    Neopixel = 0x0E,
    Touch = 0x0F,
    Encoder = 0x11
}

enum GpioCommands : byte
{
    DirSetBulk = 0x02,
    DirClrBulk = 0x03,
    Bulk = 0x04,
    BulkSet = 0x05,
    BulkClr = 0x06,
    BulkToggle = 0x07,
    IntenSet = 0x08,
    IntenClr = 0x09,
    Intflag = 0x0A,
    PullenSet = 0x0B,
    PullenClr = 0x0C
}

enum StatusCommands : byte
{
    HwId = 0x01,
    Version = 0x02,
    Options = 0x03,
    Temp = 0x04,
    SwReset = 0x7F
}

enum TimerCommands : byte
{
    Status = 0x00,
    PWM = 0x01,
    Freq = 0x02
}

enum AdcCommands : byte
{
    Status = 0x00,
    Inten = 0x02,
    IntenClr = 0x03,
    WinMode = 0x04,
    WinThresh = 0x05,
    ChannelOffset = 0x07
}

enum SercomCommands : byte
{
    Status = 0x00,
    Inten = 0x02,
    IntenClr = 0x03,
    Baud = 0x04,
    Data = 0x05
}

enum NeopixelCommands : byte
{
    Status = 0x00,
    Pin = 0x01,
    Speed = 0x02,
    BufLength = 0x03,
    Buf = 0x04,
    Show = 0x05
}

enum TouchCommands : byte
{
    ChannelOffset = 0x10
}

enum EncoderCommands : byte
{
    Status = 0x00,
    IntenSet = 0x10,
    IntenClr = 0x20,
    Position = 0x30,
    Delta = 0x40
}

public enum GpioMode : byte
{
    Input,
    Output,
    InputPullUp,
    InputPullDown
}