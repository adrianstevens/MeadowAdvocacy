using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;

namespace Meadow.Foundation.Featherwings;

public class MiniTFTJoystickFeatherwingV1
{
    /// <summary>
    /// Returns the TFT display object
    /// </summary>
    public IGraphicsDisplay Display { get; protected set; }

    /// <summary>
    /// Returns A button
    /// </summary>
    public IButton ButtonA { get; protected set; }

    /// <summary>
    /// Returns B button
    /// </summary>
    public IButton ButtonB { get; protected set; }

    /// <summary>
    /// Returns Select button (joystick click)
    /// </summary>
    public IButton JoystickSelect { get; protected set; }

    /// <summary>
    /// Returns joystick up button
    /// </summary>
    public IButton JoystickUp { get; protected set; }

    /// <summary>
    /// Returns joystick down button
    /// </summary>
    public IButton JoystickDown { get; protected set; }

    /// <summary>
    /// Returns joystick left button
    /// </summary>
    public IButton JoystickLeft { get; protected set; }

    /// <summary>
    /// Returns joystick right button
    /// </summary>
    public IButton JoystickRight { get; protected set; }


    public readonly Samd09 seesaw;

    private readonly IDigitalOutputPort? backlightPort;

    /// <summary>
    /// Create a new Mini TFT Joystick with Featherwing v1 object
    /// </summary>
    /// <param name="device">The F7 Meadow feather device connected to the wing</param>
    /// <param name="spiBus">The SPI bus connected to the wing</param>
    /// <param name="i2cBus">The I2C bus connected to the wing</param>
    public MiniTFTJoystickFeatherwingV1(IF7FeatherMeadowDevice device, ISpiBus spiBus, II2cBus i2cBus)
    {
        seesaw = new Samd09(i2cBus, 0x5E);

        //backlightPort = seesaw.CreateDigitalOutputPort(seesaw.Pins.Pin7, false);
        var resetPort = seesaw.CreateDigitalOutputPort(seesaw.Pins.Pin8, false);

        //Display = new St7735(spiBus, device.Pins.D09, device.Pins.D10, seesaw.Pins.Pin8, 80, 160, St7735.DisplayType.ST7735R_80x160_TftMini, ColorMode.Format12bppRgb444);
        //Display = new St7735(spiBus, device.Pins.D09, device.Pins.D10, null, 80, 160, St7735.DisplayType.ST7735R_80x160_TftMini, ColorMode.Format12bppRgb444);
        //blue and wrong offset 
        Display = new St7789(spiBus, device.Pins.D09, device.Pins.D10, null, 80, 160);

        return;

        ButtonA = new PollingPushButton(seesaw.Pins.Pin10, ResistorMode.InternalPullUp);
        ButtonB = new PollingPushButton(seesaw.Pins.Pin9, ResistorMode.InternalPullUp);

        JoystickSelect = new PollingPushButton(seesaw.Pins.Pin11, ResistorMode.InternalPullUp);
        JoystickUp = new PollingPushButton(seesaw.Pins.Pin2, ResistorMode.InternalPullUp);
        JoystickDown = new PollingPushButton(seesaw.Pins.Pin4, ResistorMode.InternalPullUp);
        JoystickLeft = new PollingPushButton(seesaw.Pins.Pin3, ResistorMode.InternalPullUp);
        JoystickRight = new PollingPushButton(seesaw.Pins.Pin7, ResistorMode.InternalPullUp);
    }
}

/*
 
Notes

can create St7789 after seesaw .... initializing buttons blanks the display (feels like a reset issue)

can create St7735 after seesaw .... initializing buttons blanks the display (feels like a reset issue)
- pressing button B causes display to go white ... no buttons are configured so it's something on the SAMD09
- after pressing button, deployment no longer works 
- power cycle doesn't fix it .... feels like something in the ST7789 initalization is the trick
- switching back to ST7789 doesn't fix it ....
- adding reset pin to ST7789 doesn't fix it .... which ends the reset state on true 
- manually setting reset pin to false doesn't fix it ....










*/