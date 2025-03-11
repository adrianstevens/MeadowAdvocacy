namespace Meadow.Foundation.Sensors.Camera;

public partial class ArducamBase
{
    private const byte MCU2LCD_MODE = 0x00;
    private const byte CAM2LCD_MODE = 0x01;
    private const byte LCD2MCU_MODE = 0x02;

    public const byte ARDUCHIP_TEST1 = 0x00;  //TEST register - no effect on camera 
    public const byte ARDUCHIP_FRAMES = 0x01; //the value of this register + 1 equals the number of frames to capture (e.g. 1 = 2 frames)
    public const byte ARDUCHIP_MODE = 0x02;  //Mode register - Determine who is owner of the data bus
    private const byte ARDUCHIP_TIMING = 0x03; //Sensor Interface Timing Register 
    private const byte ARDUCHIP_FIFO = 0x04;  //FIFO and I2C control
    private const byte ARDUCHIP_REV = 0x40;  //ArduCHIP revision
    public const byte ARDUCHIP_TRIG = 0x41;
    private const byte FIFO_CLEAR_MASK = 0x01;
    private const byte FIFO_START_MASK = 0x02;
    private const byte FIFO_RDPTR_RST_MASK = 0x10;
    private const byte FIFO_WRPTR_RST_MASK = 0x20;

    private const byte FIFO_SIZE1 = 0x42;  //Camera write FIFO size[7:0] for burst to read
    private const byte FIFO_SIZE2 = 0x43;  //Camera write FIFO size[15:8]
    private const byte FIFO_SIZE3 = 0x44;  //Camera write FIFO size[18:16]

    private const byte BURST_FIFO_READ = 0x3C;  //Burst FIFO read operation
    private const byte SINGLE_FIFO_READ = 0x3D;  //Single FIFO read operation

    private const byte VSYNC_MASK = 0x01;
    private const byte SHUTTER_MASK = 0x02;
    public const byte CAP_DONE_MASK = 0x08;
}
