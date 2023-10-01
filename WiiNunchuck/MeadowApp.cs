using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;

namespace ProjLab_Demo
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        DisplayController displayController;
        RgbPwmLed onboardLed;
        IProjectLabHardware projLab;

        WiiNunchuck nunchuck;

        public override Task Initialize()
        {
            Resolver.Log.LogLevel = Meadow.Logging.LogLevel.Trace;

            Resolver.Log.Info("Initialize hardware...");

            //==== RGB LED
            Resolver.Log.Info("Initializing onboard RGB LED");
            onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);
            Resolver.Log.Info("RGB LED up");

            //==== instantiate the project lab hardware
            projLab = ProjectLab.Create();

            Resolver.Log.Info($"Running on ProjectLab Hardware {projLab.RevisionString}");

            nunchuck = new WiiNunchuck(projLab.I2cBus);
            nunchuck.StartUpdating(TimeSpan.FromMilliseconds(400));

            nunchuck.AnalogStick.Updated += AnalogStick_Updated;

            //---- display controller (handles display updates)
            if (projLab.Display is { } display)
            {
                Resolver.Log.Trace("Creating DisplayController");
                displayController = new DisplayController(display);
                Resolver.Log.Trace("DisplayController up");
            }

            //---- buttons
            nunchuck.ZButton.PressStarted += (s, e) => displayController.ZButtonState = true;
            nunchuck.ZButton.PressEnded += (s, e) => displayController.ZButtonState = false;

            nunchuck.CButton.PressStarted += (s, e) => displayController.CButtonState = true;
            nunchuck.CButton.PressEnded += (s, e) => displayController.CButtonState = false;

            nunchuck.AnalogStick.Updated += (s, e) => displayController.JoystickPosition = e.New;

            //---- heartbeat
            onboardLed.StartPulse(WildernessLabsColors.PearGreen);

            Resolver.Log.Info("Initialization complete");

            return base.Initialize();
        }

        private void AnalogStick_Updated1(object sender, IChangeResult<Meadow.Peripherals.Sensors.Hid.AnalogJoystickPosition> e)
        {
            throw new NotImplementedException();
        }

        private void AnalogStick_Updated(object sender, IChangeResult<Meadow.Peripherals.Sensors.Hid.AnalogJoystickPosition> e)
        {
            throw new NotImplementedException();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            displayController?.Update();

            Resolver.Log.Info("starting blink");
            onboardLed.StartBlink(WildernessLabsColors.PearGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);

            return base.Run();
        }
    }
}