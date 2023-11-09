using HalloweenEyeball;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;

namespace F7Eyeball
{
    public class MeadowApp : App<F7FeatherV1>
    {
        EyeballController eyeballController;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var spibus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, new Meadow.Units.Frequency(48000, Meadow.Units.Frequency.UnitType.Kilohertz));
            var display = new Ili9341(spibus, Device.Pins.D13, Device.Pins.D14, Device.Pins.D15, 240, 320, Meadow.Foundation.Graphics.ColorMode.Format12bppRgb444);

            eyeballController = new EyeballController(display);

            var onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            onboardLed.StartPulse(Meadow.Foundation.Color.Red);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            eyeballController.DrawEyeball();

            while (true)
            {
                eyeballController.Delay();
                eyeballController.RandomEyeMovement();
                eyeballController.Delay();
                eyeballController.CenterEye();
            }
        }
    }
}