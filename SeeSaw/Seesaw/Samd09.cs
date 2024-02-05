using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents the SAMD09 Seesaw IO controller
/// </summary>
public partial class Samd09 : II2cPeripheral, IDigitalOutputController, IDigitalInputController
{
    /// <inheritdoc/>
    public PinDefinitions Pins { get; private set; }

    /// <inheritdoc/>
    public byte DefaultI2cAddress => 0x5E;

    private readonly II2cCommunications i2cComms;

    private readonly List<IPin> _pinsInUse = new();

    /// <summary>
    /// Create a new Samd09 object
    /// </summary>
    /// <param name="i2cBus">Bus the Seesaw is connected to</param>
    /// <param name="address">I2C address of the Seesaw device</param>
    /// <param name="resetOnInit">Whether to do a software reset on init</param>
    public Samd09(II2cBus i2cBus, byte address = 0x5E)
    {
        Pins = new PinDefinitions(this);

        i2cComms = new I2cCommunications(i2cBus, address);

        Reset();

        var data = new byte[1];

        for (int i = 0; i < 20; i++)
        {
            i2cComms.Exchange(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.HwId }, data);
            var hardwareType = (HardwareId)data[0];

            if (hardwareType == HardwareId.ATSAMD09)
            {
                return;
            }

            Console.WriteLine($"Hardware type {hardwareType} not found, retrying");

            Thread.Sleep(50);
        }

        throw new Exception("Unable to find SAMD09");
    }

    /// <summary>
    /// Reset the Samd09 device
    /// </summary>
    public void Reset()
    {
        //     i2cComms.Write(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.SwReset, 0xFF });

        i2cComms.WriteRegister((byte)BaseAddresses.Status, new byte[] { (byte)StatusCommands.SwReset, 0xFF });
    }

    private void SendGpioCommand(GpioCommands command, byte[] data)
    {
        Console.WriteLine($"SendGpioCommand {command} {data[0]} {data[1]} {data[2]} {data[3]}");

        i2cComms.WriteRegister((byte)BaseAddresses.GPIO, new byte[] { (byte)command, data[0], data[1], data[2], data[3] });

        //    i2cComms.Exchange(new byte[] { (byte)BaseAddresses.GPIO, (byte)command }, data);
    }

    uint GetOptions()
    {
        byte[] data = new byte[4];
        i2cComms.Exchange(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.Options }, data);
        return ((uint)data[0] << 24) | ((uint)data[1] << 16) | ((uint)data[2] << 8) | data[3];
    }

    uint GetVersion()
    {
        byte[] data = new byte[4];
        i2cComms.Exchange(new byte[] { (byte)BaseAddresses.Status, (byte)StatusCommands.Version }, data);
        return ((uint)data[0] << 24) | ((uint)data[1] << 16) | ((uint)data[2] << 8) | (uint)data[3];
    }

    public void SetPinMode(IPin pin, GpioMode mode)
    {
        byte key = (byte)pin.Key;

        if (key >= 32)
        {
            PinModeBulk(0, 1u << (key - 32), mode);
        }
        else
        {
            PinModeBulk(1u << key, mode);
        }
    }

    /// <summary>
    /// Set the mode of multiple GPIO pins at once.
    /// </summary>
    /// <param name="pins">A bitmask of the pins to write. For example, passing 0b0110 will set the mode of pins 2 and 3.</param>
    /// <param name="mode">The mode to set the pins to. One of INPUT, OUTPUT, or INPUT_PULLUP.</param>
    void PinModeBulk(uint pins, GpioMode mode)
    {
        Console.WriteLine($"PinModeBulk {pins} {mode}");

        byte[] data = {
            (byte)(pins >> 24),
            (byte)(pins >> 16),
            (byte)(pins >> 8),
            (byte)pins
        };

        switch (mode)
        {
            case GpioMode.Output:
                SendGpioCommand(GpioCommands.DirSetBulk, data);
                break;
            case GpioMode.Input:
                SendGpioCommand(GpioCommands.DirClrBulk, data);
                break;
            case GpioMode.InputPullUp:
                SendGpioCommand(GpioCommands.DirClrBulk, data);
                SendGpioCommand(GpioCommands.PullenSet, data);
                SendGpioCommand(GpioCommands.BulkSet, data);
                break;
            case GpioMode.InputPullDown:
                SendGpioCommand(GpioCommands.DirClrBulk, data);
                SendGpioCommand(GpioCommands.PullenSet, data);
                SendGpioCommand(GpioCommands.BulkClr, data);
                break;
        }
    }

    /// <summary>
    /// Set the mode of multiple GPIO pins at once. This supports both ports A and B.
    /// </summary>
    /// <param name="pinsA">A bitmask of the pins to write on port A. For example, passing 0b0110 will set the mode of pins 2 and 3.</param>
    /// <param name="pinsB">A bitmask of the pins to write on port B.</param>
    /// <param name="mode">The mode to set the pins to. One of INPUT, OUTPUT, or INPUT_PULLUP.</param>
    void PinModeBulk(uint pinsA, uint pinsB, GpioMode mode)
    {
        byte[] data = {
        (byte)(pinsA >> 24),
        (byte)(pinsA >> 16),
        (byte)(pinsA >> 8),
        (byte)pinsA,
        (byte)(pinsB >> 24),
        (byte)(pinsB >> 16),
        (byte)(pinsB >> 8),
        (byte)pinsB
    };

        switch (mode)
        {
            case GpioMode.Output:
                SendGpioCommand(GpioCommands.DirSetBulk, data);
                break;
            case GpioMode.Input:
                SendGpioCommand(GpioCommands.DirClrBulk, data);
                break;
            case GpioMode.InputPullUp:
                SendGpioCommand(GpioCommands.DirClrBulk, data);
                SendGpioCommand(GpioCommands.PullenSet, data);
                SendGpioCommand(GpioCommands.BulkSet, data);
                break;
            case GpioMode.InputPullDown:
                SendGpioCommand(GpioCommands.DirClrBulk, data);
                SendGpioCommand(GpioCommands.PullenSet, data);
                SendGpioCommand(GpioCommands.BulkClr, data);
                break;
        }
    }

    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        lock (_pinsInUse)
        {
            if (_pinsInUse.Contains(pin))
            {
                throw new PortInUseException($"{GetType().Name} pin {pin.Name} is already in use.");
            }
            var port = new DigitalOutputPort(this, pin, initialState);

            _pinsInUse.Add(pin);

            SetPinMode(pin, GpioMode.Output);

            port.Disposed += (s, e) =>
            {
                lock (_pinsInUse)
                {
                    _pinsInUse.Remove(pin);
                }
            };

            Console.WriteLine($"Created output port {pin.Key}");

            return port;
        }
    }

    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
    {
        lock (_pinsInUse)
        {
            if (_pinsInUse.Contains(pin))
            {
                throw new PortInUseException($"{GetType().Name} pin {pin.Name} is already in use.");
            }
            var port = new DigitalInputPort(this, pin)
            {
                Resistor = resistorMode
            };

            switch (resistorMode)
            {
                case ResistorMode.Disabled:
                    SetPinMode(pin, GpioMode.Input);
                    break;
                case ResistorMode.InternalPullUp:
                    SetPinMode(pin, GpioMode.InputPullUp);
                    break;
                case ResistorMode.InternalPullDown:
                    SetPinMode(pin, GpioMode.InputPullDown);
                    break;
            }

            _pinsInUse.Add(pin);

            port.Disposed += (s, e) =>
            {
                lock (_pinsInUse)
                {
                    _pinsInUse.Remove(pin);
                }
            };

            Console.WriteLine($"Created input port {pin.Key} with resistor mode {resistorMode}");

            return port;
        }
    }

    /// <summary>
    /// Retrieves the state of a pin
    /// </summary>
    /// <param name="pin">The pin to query</param>
    public bool GetState(IPin pin)
    {
        //Console.WriteLine($"GetState {pin.Key}");

        byte key = (byte)pin.Key;
        if (key >= 32)
        {
            return DigitalReadBulkB(1u << (key - 32)) != 0;
        }
        else
        {
            return DigitalReadBulk(1u << key) != 0;
        }
    }

    /// <summary>
    /// Sets the state of a pin
    /// </summary>
    /// <param name="pin">The pin to affect</param>
    /// <param name="state"><b>True</b> to set the pin state high, <b>False</b> to set it low</param>
    public void SetState(IPin pin, bool state)
    {
        Console.WriteLine($"SetState {pin.Key} {state}");

        byte key = (byte)pin.Key;

        if (key >= 32)
        {
            DigitalWriteBulk(0, 1u << (key - 32), (byte)(state ? 1 : 0));
        }
        else
        {
            DigitalWriteBulk(1u << key, (byte)(state ? 1 : 0));
        }
    }

    /// <summary>
    /// Read the status of multiple pins on port A.
    /// </summary>
    /// <param name="pins">A bitmask of the pins to read. For example, passing 0b0110 will return the values of pins 2 and 3.</param>
    /// <returns>The status of the passed pins. If 0b0110 was passed and pin 2 is high and pin 3 is low, 0b0010 (decimal number 2) will be returned.</returns>
    public uint DigitalReadBulk(uint pins)
    {
        byte[] data = new byte[4];

        i2cComms.Exchange(new byte[] { (byte)BaseAddresses.GPIO, (byte)GpioCommands.Bulk }, data);

        uint ret = ((uint)data[0] << 24) | ((uint)data[1] << 16) | ((uint)data[2] << 8) | (uint)data[3];
        return ret & pins;
    }

    /// <summary>
    /// Read the status of multiple pins on port B.
    /// </summary>
    /// <param name="pins">A bitmask of the pins to read.</param>
    /// <returns>The status of the passed pins. If 0b0110 was passed and pin 2 is high and pin 3 is low, 0b0010 (decimal number 2) will be returned.</returns>
    public uint DigitalReadBulkB(uint pins)
    {
        byte[] data = new byte[8];
        i2cComms.Exchange(new byte[] { (byte)BaseAddresses.GPIO, (byte)GpioCommands.Bulk }, data);

        uint ret = ((uint)data[4] << 24) | ((uint)data[5] << 16) | ((uint)data[6] << 8) | (uint)data[7];
        return ret & pins;
    }

    /// <summary>
    /// Write a value to multiple GPIO pins at once.
    /// </summary>
    /// <param name="pins">A bitmask of the pins to write. For example, passing 0b0110 will write the passed value to pins 2 and 3.</param>
    /// <param name="value">Pass HIGH to set the output on the passed pins to HIGH, low to set the output on the passed pins to LOW.</param>
    public void DigitalWriteBulk(uint pins, byte value)
    {
        byte[] data = {
            (byte)(pins >> 24),
            (byte)(pins >> 16),
            (byte)(pins >> 8),
            (byte)pins
        };

        if (value != 0)
        {
            i2cComms.Exchange(new byte[] { (byte)BaseAddresses.GPIO, (byte)GpioCommands.BulkSet }, data);
        }
        else
        {
            i2cComms.Exchange(new byte[] { (byte)BaseAddresses.GPIO, (byte)GpioCommands.BulkClr }, data);
        }
    }

    /// <summary>
    /// Write a value to multiple GPIO pins at once. This supports both ports A and B.
    /// </summary>
    /// <param name="pinsA">A bitmask of the pins to write on port A. For example, passing 0b0110 will write the passed value to pins 2 and 3.</param>
    /// <param name="pinsB">A bitmask of the pins to write on port B.</param>
    /// <param name="value">Pass HIGH to set the output on the passed pins to HIGH, low to set the output on the passed pins to LOW.</param>
    public void DigitalWriteBulk(uint pinsA, uint pinsB, byte value)
    {
        byte[] data = {
            (byte)(pinsA >> 24),
            (byte)(pinsA >> 16),
            (byte)(pinsA >> 8),
            (byte)pinsA,
            (byte)(pinsB >> 24),
            (byte)(pinsB >> 16),
            (byte)(pinsB >> 8),
            (byte)pinsB
        };

        if (value != 0)
        {
            i2cComms.Exchange(new byte[] { (byte)BaseAddresses.GPIO, (byte)GpioCommands.BulkSet }, data);
        }
        else
        {
            i2cComms.Exchange(new byte[] { (byte)BaseAddresses.GPIO, (byte)GpioCommands.BulkClr }, data);
        }
    }
}