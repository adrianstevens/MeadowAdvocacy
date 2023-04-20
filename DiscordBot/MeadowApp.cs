using Discord;
using Discord.WebSocket;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace DeskClock
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        string TOKEN = "MTA5NzY2MjUyNjM2MTE4NjM1NQ.G-zxvn.BZKoHMr6yZ4t-QmMdRSXo9t2Y0sLpPFt2Shlgo";

        MicroGraphics graphics;

        IWiFiNetworkAdapter wifi;

        private const string WIFI_NAME = "BunnyMesh";
        private const string WIFI_PASSWORD = "zxpvi29wt8";

        public override Task Run()
        {

            return base.Run();
        }

        private async void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            Console.WriteLine("WiFi connected...");
        }

        public async override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            await InitWifi();

            await InitDiscord();
        }

        async Task InitDiscord()
        {
            Console.WriteLine("Init Discord...");

            var mySock = new DiscordSocketClient();
            mySock.Connected += MySock_Connected;
            mySock.Ready += MySock_Ready;

            Console.WriteLine("Logging in...");
            await mySock.LoginAsync(TokenType.Bot, TOKEN);

            Console.WriteLine("Start...");
            await mySock.StartAsync();

            Console.WriteLine("Discord init complete");
        }

        private async Task MySock_Ready()
        {
            Console.WriteLine("Ready");
        }

        private Task MySock_Connected()
        {
            Console.WriteLine("Socket to me");

            return Task.CompletedTask;
        }

        async Task InitWifi()
        {
            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            wifi.NetworkConnected += Wifi_NetworkConnected;

            try
            {
                // connect to the wifi network.
                Resolver.Log.Info($"Connecting to WiFi Network {WIFI_NAME}");

                await wifi.Connect(WIFI_NAME, WIFI_PASSWORD, TimeSpan.FromSeconds(45));
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect: {ex.Message}");
            }

            Console.WriteLine("WiFi Init complete");
        }
    }
}