using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;

namespace WaitOnNetwork
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        RgbPwmLed onboardLed;

        Task networkStartedTask;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            if (wifi.IsConnected)
            {
                networkStartedTask = Task.CompletedTask;
            }
            else
            {
                wifi.NetworkConnected += Wifi_NetworkConnected;
            }

            return Task.CompletedTask;
        }

        private void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            networkStartedTask = Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Don't run until we're connected...");

            await networkStartedTask;

            await CycleColors(TimeSpan.FromMilliseconds(1000));
        }

        async Task CycleColors(TimeSpan duration)
        {
            Resolver.Log.Info("Cycle colors...");

            while (true)
            {
                await ShowColorPulse(Color.Blue, duration);
                await ShowColorPulse(Color.Cyan, duration);
                await ShowColorPulse(Color.Green, duration);
                await ShowColorPulse(Color.GreenYellow, duration);
                await ShowColorPulse(Color.Yellow, duration);
                await ShowColorPulse(Color.Orange, duration);
                await ShowColorPulse(Color.OrangeRed, duration);
                await ShowColorPulse(Color.Red, duration);
                await ShowColorPulse(Color.MediumVioletRed, duration);
                await ShowColorPulse(Color.Purple, duration);
                await ShowColorPulse(Color.Magenta, duration);
                await ShowColorPulse(Color.Pink, duration);
            }
        }

        async Task ShowColorPulse(Color color, TimeSpan duration)
        {
            await onboardLed.StartPulse(color, duration / 2);
            await Task.Delay(duration);
        }
    }
}