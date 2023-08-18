using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace AirQualityTracker
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            projLab.Display.Fill(Color.Red);
            projLab.Display.Show();

            return base.Run();
        }

        public override async Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            var client = projLab.GetModbusRtuClient(9600);

            var sensor = new Y4000(client, 1);

            await sensor.Initialize();

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var address = await sensor.GetISDN();
                    Console.WriteLine($"Address: {address}");

                    var serial = await sensor.GetSerialNumber();
                    Console.WriteLine($"Serial: {serial}");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Exception: {ex}");
                }

                await Task.Delay(5000);
            }

            sensor.Updated += Sensor_Updated;

            sensor.StartUpdating();

            Console.WriteLine("Init complete");
        }

        private void Sensor_Updated(object sender, IChangeResult<(Meadow.Units.ConcentrationInWater? DisolvedOxygen, Meadow.Units.ConcentrationInWater? Chlorophyl, Meadow.Units.ConcentrationInWater? BlueGreenAlgae, Meadow.Units.Conductivity? ElectricalConductivity, Meadow.Units.PotentialHydrogen? PH, Meadow.Units.Turbidity? Turbidity, Meadow.Units.Temperature? Temperature, Meadow.Units.Voltage? OxidationReductionPotential)> e)
        {
            Console.WriteLine($"Temp: {e.New.Temperature.Value.Celsius}C");
            Console.WriteLine($"pH: {e.New.PH.Value}pH");
            Console.WriteLine($"Disolved O2: {e.New.DisolvedOxygen.Value}");
        }
    }
}