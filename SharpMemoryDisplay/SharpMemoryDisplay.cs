using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a Sharp Memory Display
    /// </summary>
    public class SharpMemoryDisplay : IPixelDisplay, ISpiPeripheral, IDisposable
    {
        private const byte SHARPMEM_BIT_WRITECMD = 0x01; // 0x80 in LSB format
        private const byte SHARPMEM_BIT_VCOM = 0x02;     // 0x40 in LSB format
        private const byte SHARPMEM_BIT_CLEAR = 0x04;    // 0x20 in LSB format

        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public ColorMode SupportedColorModes => ColorMode.Format1bpp;

        /// <inheritdoc/>
        public int Height => 144;

        /// <inheritdoc/>
        public int Width => 168;

        /// <inheritdoc/>
        public IPixelBuffer PixelBuffer => imageBuffer;

        private byte _sharpmem_vcom = SHARPMEM_BIT_VCOM;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(2000, Frequency.UnitType.Kilohertz);

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
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        readonly IDigitalOutputPort chipSelectPort;

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications spiComms;

        /// <summary>
        /// Buffer to hold display data
        /// </summary>
        protected Buffer1bpp imageBuffer;

        /// <summary>
        /// Create a SharpMemoryDisplay object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        public SharpMemoryDisplay(ISpiBus spiBus, IPin chipSelectPin) :
            this(spiBus, chipSelectPin.CreateDigitalOutputPort())
        {
        }

        /// <summary>
        /// Create a new SharpMemoryDisplay object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        public SharpMemoryDisplay(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
        {
            imageBuffer = new Buffer1bpp(Width, Height);
            this.chipSelectPort = chipSelectPort;

            spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode, csMode: ChipSelectMode.ActiveHigh);

            Initialize();
        }

        private void Initialize()
        {
            Clear();
            Show();
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <remarks>
        /// Clears the internal memory buffer 
        /// </remarks>
        /// <param name="updateDisplay">If true, it will force a display update</param>
        public void Clear(bool updateDisplay = false)
        {
            Array.Clear(imageBuffer.Buffer, 0, imageBuffer.ByteCount);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Draw pixel at location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="enabled">True = turn on pixel, false = turn off pixel</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            imageBuffer.SetPixel(x, y, enabled);
        }

        /// <summary>
        /// Invert pixel at a location
        /// </summary>
        /// <param="x">x position in pixels</param>
        /// <param="y">y position in pixels</param>
        public void InvertPixel(int x, int y)
        {
            imageBuffer.InvertPixel(x, y);
        }

        /// <summary>
        /// Draw pixel at location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="color">any value other than black will make the pixel visible</param>
        public void DrawPixel(int x, int y, Color color)
        {
            DrawPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Update the display
        /// </summary>
        public void Show()
        {
            var commandBuffer = new byte[1];
            commandBuffer[0] = ReverseBits((byte)(_sharpmem_vcom | SHARPMEM_BIT_WRITECMD));
            spiComms.Write(commandBuffer);
            ToggleVCOM();

            int bytesPerLine = Width / 8;
            byte[] lineBuffer = new byte[bytesPerLine + 2];

            for (int i = 0; i < Height; i++)
            {
                lineBuffer[0] = ReverseBits((byte)(i + 1));
                for (int j = 0; j < bytesPerLine; j++)
                {
                    lineBuffer[1 + j] = ReverseBits(imageBuffer.Buffer[i * bytesPerLine + j]);
                }
                lineBuffer[bytesPerLine + 1] = 0x00;
                spiComms.Write(lineBuffer);
            }

            spiComms.Write(new byte[] { 0x00 });
        }

        private void ToggleVCOM()
        {
            _sharpmem_vcom = (byte)(_sharpmem_vcom == 0x00 ? SHARPMEM_BIT_VCOM : 0x00);
        }

        /// <summary>
        /// Update a region of the display 
        /// Currently always does a full screen update for this display
        /// </summary>
        /// <param name="left">The left position in pixels</param>
        /// <param name="top">The top position in pixels</param>
        /// <param name="right">The right position in pixels</param>
        /// <param name="bottom">The bottom position in pixels</param>
        public void Show(int left, int top, int right, int bottom)
        {
            Show();
        }

        /// <summary>
        /// Fill with color 
        /// </summary>
        /// <param name="fillColor">color - converted to on/off</param>
        /// <param name="updateDisplay">should refresh display</param>
        public void Fill(Color fillColor, bool updateDisplay = false)
        {
            imageBuffer.Clear(fillColor.Color1bpp);

            if (updateDisplay) { Show(); }
        }

        /// <summary>
        /// Fill region with color
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width of region</param>
        /// <param name="height">height of region</param>
        /// <param name="fillColor">color - converted to on/off</param>
        public void Fill(int x, int y, int width, int height, Color fillColor)
        {
            imageBuffer.Fill(x, y, width, height, fillColor);
        }

        /// <summary>
        /// Draw buffer at location
        /// </summary>
        /// <param name="x">x position in pixels</param>
        /// <param name="y">y position in pixels</param>
        /// <param name="displayBuffer">buffer to draw</param>
        public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
        {
            imageBuffer.WriteBuffer(x, y, displayBuffer);
        }

        /// <summary>
        /// Clears the display buffer without outputting to the display
        /// </summary>
        public void ClearDisplayBuffer()
        {
            Array.Clear(imageBuffer.Buffer, 0, imageBuffer.ByteCount);
        }

        /// <summary>
        /// Clears the screen
        /// </summary>
        public void ClearDisplay()
        {
            ClearDisplayBuffer();

            var commandBuffer = new byte[2];
            commandBuffer[0] = ReverseBits((byte)(_sharpmem_vcom | SHARPMEM_BIT_CLEAR));
            commandBuffer[1] = 0x00;
            spiComms.Write(commandBuffer);
            ToggleVCOM();
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    chipSelectPort?.Dispose();
                }

                IsDisposed = true;
            }
        }

        /// <summary>
        /// Reverse the bits in a byte
        /// </summary>
        /// <param name="b">The byte to reverse</param>
        /// <returns>The byte with its bits reversed</returns>
        private static byte ReverseBits(byte b)
        {
            b = (byte)((b * 0x0202020202 & 0x010884422010) % 1023);
            return b;
        }
    }
}