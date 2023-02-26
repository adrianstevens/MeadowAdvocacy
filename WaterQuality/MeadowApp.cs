using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Threading.Tasks;

namespace AirQualityTracker
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        IProjectLabHardware projLab;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            projLab.Display.Clear(Color.Red);
            projLab.Display.Show();

            return base.Run();
        }

        public override async Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            var client = projLab.GetModbusRtuClient(9600);

            var sensor = new Y4000(client, 1);
            sensor.Initialize();

            while (true)
            {
                try
                {
                    var address = await sensor.GetISDN();
                   // var serial = await sensor.GetSerialNumber();
                    Console.WriteLine($"{address}");
                }
                catch (Exception ex) 
                {
                    Resolver.Log.Error($"Exception: {ex}");
                }

                await Task.Delay(5000);
            }

            Console.WriteLine("Init complete");
        }
    }
}