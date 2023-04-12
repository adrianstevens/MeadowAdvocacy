using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyboardDrawing
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        MicroGraphics graphics;

        KeyboardWing keyboardWing;

        public override Task Run()
        {
            graphics.Clear();
            graphics.PenColor = Color.Cyan;

            var touchScreen = keyboardWing.TouchScreen;

            Point3d? lastPoint = null;
            Point3d newPoint;

            while (true)
            {
                newPoint = touchScreen.GetPoint();
                if (lastPoint == null) { lastPoint = newPoint; }

                graphics.DrawLine(lastPoint.Value.X, lastPoint.Value.Y, newPoint.X, newPoint.Y);
                graphics.Show();
                lastPoint = newPoint;

                Thread.Sleep(0);
            }
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            var spiBus = Device.CreateSpiBus(new Frequency(48000, Frequency.UnitType.Kilohertz));

            keyboardWing = new KeyboardWing(
                spiBus: spiBus,
                i2cBus: i2cBus,
                keyboardPin: Device.Pins.D13, //should be D10
                displayChipSelectPin: Device.Pins.D11,
                displayDcPin: Device.Pins.D12,
                lightSensorPin: Device.Pins.A05);

            graphics = new MicroGraphics(keyboardWing.Display)
            {
                Rotation = RotationType._90Degrees,
                CurrentFont = new Font12x16()
            };

            keyboardWing.TouchScreen.Rotation = RotationType._90Degrees;

            return Task.CompletedTask;
        }
    }
}