using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Represents and RD-03D motion sensor
/// </summary>
public abstract partial class Rd03d
{
    readonly ISerialMessagePort? serialMessagePort;

    static readonly byte[] suffixDelimiter = { 13 }; //place holder, this might be wrong
    static readonly int portSpeed = 256000;

    /// <summary>
    /// Did we create the port(s) used by the peripheral
    /// </summary>
    readonly bool createdPorts = false;

    DateTime lastUpdate = DateTime.MinValue;

    /// <summary>
    /// Creates a new Rd03d object communicating over serial
    /// </summary>
    /// <param name="device">The device connected to the sensor</param>
    /// <param name="serialPortName">The serial port name</param>
    public Rd03d(IMeadowDevice device, SerialPortName serialPortName) :
        this(device.CreateSerialMessagePort(serialPortName, suffixDelimiter, false, baudRate: portSpeed))
    {
        createdPorts = true;
    }

    /// <summary>
    /// Creates a new Rd03d object communicating over serial
    /// </summary>
    /// <param name="serialMessage">The serial message port</param>
    public Rd03d(ISerialMessagePort serialMessage)
    {
        serialMessagePort = serialMessage;
        serialMessagePort.MessageReceived += SerialMessagePort_MessageReceived;
    }

    private void SerialMessagePort_MessageReceived(object sender, SerialMessageData e)
    {
        Console.WriteLine($"Received: {e.Message}");
    }
}