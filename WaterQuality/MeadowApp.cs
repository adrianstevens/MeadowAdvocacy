using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Hardware;
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

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            IDigitalOutputPort chipSelectPort, resetPort;

            
            if (projLab is ProjectLabHardwareV2 { } projlabV2)
            {
            }
            else
            {
            }

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}