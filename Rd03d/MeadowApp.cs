using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using System;
using System.Threading.Tasks;

namespace ArducamMini
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();


            var sensor = new Rd03d(Device, projLab.GroveUart.SerialPortName);
            sensor.TargetsUpdated += (s, targets) =>
            {
                foreach (var t in targets)
                {
                    Resolver.Log.Info($"({t.X_mm},{t.Y_mm}) mm  v={t.Speed_cms} cm/s  d={t.Distance_mm} mm");
                }
            };
            sensor.Start();


            Console.WriteLine("initialized");


            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Console.WriteLine("Run...");
            return Task.CompletedTask;
        }
    }
}