using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace WaterQualityTracker
{
    internal class WaterQualityController
    {
        DisplayController displayController;

        Y4000 sensor;

        public async Task Initialize(IProjectLabHardware projectLab)
        {
            displayController = new DisplayController(projectLab.Display);

            var client = projectLab.GetModbusRtuClient(9600);

            sensor = new Y4000(client, 1);

            sensor.Updated += Y4000Updated;

            await sensor.Initialize();
        }

        public void Run()
        {
            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        private void Y4000Updated(object sender,
            Meadow.IChangeResult<(Meadow.Units.ConcentrationInWater? DissolvedOxygen,
                Meadow.Units.ConcentrationInWater? Chlorophyl,
                Meadow.Units.ConcentrationInWater? BlueGreenAlgae,
                Meadow.Units.Conductivity? ElectricalConductivity,
                Meadow.Units.PotentialHydrogen? PH,
                Meadow.Units.Turbidity? Turbidity,
                Meadow.Units.Temperature? Temperature,
                Meadow.Units.Voltage? OxidationReductionPotential)> e)
        {
            displayController.UpdateDisplay(e.New);

            Console.WriteLine($"Temp: {e.New.Temperature.Value.Celsius:0.0}C");
            Console.WriteLine($"pH: {e.New.PH.Value:0.0}pH");
            Console.WriteLine($"Disolved O2: {e.New.DissolvedOxygen.Value:0.0}");
        }
    }
}