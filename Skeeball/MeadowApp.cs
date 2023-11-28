using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace Skeeball;

// Change F7FeatherV2 to F7FeatherV1 for V1.x boards
public class MeadowApp : App<F7CoreComputeV2>
{
    SkeeBallCoordinator skeeball;

    SkeeballHardware hardware;

    IWiFiNetworkAdapter wifi;

    private const string WIFI_NAME = "";
    private const string WIFI_PASSWORD = "";

    public override async Task Initialize()
    {
        Resolver.Log.Info("Initialize ...");

        hardware = new SkeeballHardware();
        await hardware.Initialize();

        skeeball = new SkeeBallCoordinator(hardware);
        skeeball.Initialize();

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
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run ...");
        await skeeball.Run();
    }

    private async void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
    {
    }
}