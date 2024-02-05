using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Samd09
{
    /// <summary>
    /// Pin definitions for Samd09 IO expanders
    /// </summary>
    public class PinDefinitions : IPinDefinitions
    {
        /// <summary>
        /// The controller for the pins
        /// </summary>
        public IPinController? Controller { get; set; }

        /// <summary>
        /// List of pins
        /// </summary>
        public IList<IPin> AllPins { get; } = new List<IPin>();

        /// <summary>
        /// Pin 0
        /// </summary>
        public IPin Pin0 => new Pin(
            Controller,
            nameof(Pin0), (byte)0x00,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin0)),
            }
        );

        /// <summary>
        /// Pin 1
        /// </summary>
        public IPin Pin1 => new Pin(
            Controller,
            nameof(Pin1), (byte)0x01,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin1)),
            }
        );

        /// <summary>
        /// Pin 2
        /// </summary>
        public IPin Pin2 => new Pin(
            Controller,
            nameof(Pin2), (byte)0x02,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin2)),
            }
        );

        /// <summary>
        /// Pin 3
        /// </summary>
        public IPin Pin3 => new Pin(
            Controller,
            nameof(Pin3), (byte)0x03,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin3)),
            }
        );

        /// <summary>
        /// Pin 4
        /// </summary>
        public IPin Pin4 => new Pin(
            Controller,
            nameof(Pin4), (byte)0x04,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin4)),
            }
        );

        /// <summary>
        /// Pin 3
        /// </summary>
        public IPin Pin5 => new Pin(
            Controller,
            nameof(Pin5), (byte)0x05,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin5)),
            }
        );

        /// <summary>
        /// Pin 6
        /// </summary>
        public IPin Pin6 => new Pin(
            Controller,
            nameof(Pin6), (byte)0x06,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin6)),
            }
        );

        /// <summary>
        /// Pin 3
        /// </summary>
        public IPin Pin7 => new Pin(
            Controller,
            nameof(Pin7), (byte)0x07,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin7)),
            }
        );

        /// <summary>
        /// Pin 8
        /// </summary>
        public IPin Pin8 => new Pin(
            Controller,
            nameof(Pin8), (byte)0x08,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin8)),
            }
        );

        /// <summary>
        /// Pin 9
        /// </summary>
        public IPin Pin9 => new Pin(
            Controller,
            nameof(Pin9), (byte)0x09,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin9)),
            }
        );

        /// <summary>
        /// Pin 10
        /// </summary>
        public IPin Pin10 => new Pin(
            Controller,
            nameof(Pin10), (byte)10,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin10)),
            }
        );

        /// <summary>
        /// Pin 11
        /// </summary>
        public IPin Pin11 => new Pin(
            Controller,
            nameof(Pin11), (byte)11,
            new List<IChannelInfo> {
                new DigitalChannelInfo(nameof(Pin11)),
            }
        );

        /// <summary>
        /// Create a new PinDefinitions object
        /// </summary>
        public PinDefinitions(Samd09 controller)
        {
            Controller = controller;
            InitAllPins();
        }

        /// <summary>
        /// Initalize all pins
        /// </summary>
        protected void InitAllPins()
        {
            // add all our pins to the collection
            AllPins.Add(Pin0);
            AllPins.Add(Pin1);
            AllPins.Add(Pin2);
            AllPins.Add(Pin3);
            AllPins.Add(Pin4);
            AllPins.Add(Pin5);
            AllPins.Add(Pin6);
            AllPins.Add(Pin7);
            AllPins.Add(Pin8);
            AllPins.Add(Pin9);
            AllPins.Add(Pin10);
            AllPins.Add(Pin11);
        }

        /// <summary>
        /// Get Pins
        /// </summary>
        /// <returns>IEnumerator of IPin with all pins</returns>
        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}