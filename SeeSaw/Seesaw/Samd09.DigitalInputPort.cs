using Meadow.Hardware;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Samd09
{
    /// <summary>
    /// A SAMD09-specific implementation of the IInputPort
    /// </summary>
    public class DigitalInputPort : DigitalInputPortBase
    {
        private ResistorMode _resistorMode = ResistorMode.Disabled;

        internal event EventHandler Disposed = default!;

        /// <summary>
        /// The the SAMD09 peripheral controlling the port
        /// </summary>
        public Samd09 Peripheral { get; }

        /// <summary>
        /// Creates a DigitalInputPort instance
        /// </summary>
        /// <param name="peripheral">the Pca9671 instance</param>
        /// <param name="pin">The IPIn to use for the port</param>
        public DigitalInputPort(Samd09 peripheral, IPin pin)
            : base(pin, pin.SupportedChannels.OfType<IDigitalChannelInfo>().First())
        {
            Peripheral = peripheral;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public override ResistorMode Resistor
        {
            get => _resistorMode;
            set => _resistorMode = value;
        }

        /// <inheritdoc/>
        public override bool State => Peripheral.GetState(Pin);
    }
}