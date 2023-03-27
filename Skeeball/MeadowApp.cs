using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;
using WildernessLabs.Hardware.Juego;

namespace Skeeball
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        DisplayController displayController;

        MicroGraphics graphics;

        IJuegoHardware juego;

        IWiFiNetworkAdapter wifi;

        private const string WIFI_NAME = "BunnyMesh";
        private const string WIFI_PASSWORD = "zxpvi29wt8";


        public override Task Run()
        {
            displayController.DrawTitle();

            return base.Run();
        }

        private async void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {

        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();

            graphics = new MicroGraphics(juego.Display)
            {
                CurrentFont = new Font4x8(),
                Rotation = RotationType._270Degrees
            };

            displayController = new DisplayController(graphics);

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            wifi.NetworkConnected += Wifi_NetworkConnected;

            /* ... will use it later but commented out for perf
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
            */

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}