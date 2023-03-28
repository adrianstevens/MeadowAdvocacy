using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeskClock
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        MicroGraphics graphics;

        Max7219 display;

        IWiFiNetworkAdapter wifi;

        private const string WIFI_NAME = "BunnyMesh";
        private const string WIFI_PASSWORD = "zxpvi29wt8";

        void DrawTime(int hour, int minute, int second)
        {
            graphics.Clear();

            string hr = $"{hour}";

            if (hr.Length == 1) { hr = " " + hr; }

            graphics.DrawText(0, 1, hr);
            graphics.DrawPixel(9, 2);
            graphics.DrawPixel(9, 5);
            graphics.DrawText(12, 1, $"{minute:D2}");
            graphics.DrawPixel(21, 2);
            graphics.DrawPixel(21, 5);
            graphics.DrawText(24, 1, $"{second:D2}");

            graphics.Show();
        }

        public override Task Run()
        {
            while (true)
            {
                var hour = DateTime.Now.Hour;
                if (hour == 0) { hour = 24; }

                DrawTime(hour, DateTime.Now.Minute, DateTime.Now.Second);
                Thread.Sleep(1000);
            }

            return base.Run();
        }

        private async void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {


            UpdateClock();
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            display = new Max7219(Device.CreateSpiBus(), Device.Pins.D00, 4, Max7219.Max7219Mode.Display);

            graphics = new MicroGraphics(display)
            {
                Rotation = RotationType._90Degrees,
                CurrentFont = new Font4x8(),
            };

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            wifi.NetworkConnected += Wifi_NetworkConnected;

            try
            {
                // connect to the wifi network.
                Resolver.Log.Info($"Connecting to WiFi Network {WIFI_NAME}");

                _ = wifi.Connect(WIFI_NAME, WIFI_PASSWORD, TimeSpan.FromSeconds(45));
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect: {ex.Message}");
            }

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        async Task UpdateClock()
        {
            //    var result = await Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD);

            var today = await ClockService.GetTime();

            Device.PlatformOS.SetClock(today.Subtract(new TimeSpan(7, 0, 0)));
        }
    }
}