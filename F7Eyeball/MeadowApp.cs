using HalloweenEyeball;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace F7Eyeball
{
    public class MeadowApp : App<F7FeatherV1>
    {
        EyeballController eyeballController;

        Hcsens0040 motionSensor;

        readonly bool demoMode = false;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            motionSensor = new Hcsens0040(Device.Pins.D04);
            motionSensor.OnMotionDetected += MotionSensor_OnMotionDetected;

            var spibus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, new Frequency(48000, Frequency.UnitType.Kilohertz));
            var display = new Ili9341(spibus, Device.Pins.D13, Device.Pins.D14, Device.Pins.D15, 240, 320, ColorMode.Format12bppRgb444);

            eyeballController = new EyeballController(display);

            var onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            onboardLed.StartPulse(Color.Red);

            return Task.CompletedTask;
        }

        private void MotionSensor_OnMotionDetected(object sender)
        {
            Console.WriteLine("Motion detected");
            eyeballController.RandomEyeMovement();
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");

            eyeballController.DrawEyeball();

            while (demoMode)
            {
                eyeballController.Delay();
                eyeballController.RandomEyeMovement();
                eyeballController.Delay();
                eyeballController.CenterEye();
            }

            return Task.CompletedTask;
        }
    }
}