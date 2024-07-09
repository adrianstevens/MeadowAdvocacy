using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.mikroBUS.Sensors.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Threading.Tasks;

namespace LineChart
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        CGNSS10 gps;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            gps = new CGNSS10(projLab.MikroBus2.SpiBus,
                              projLab.MikroBus2.Pins.CS,
                              projLab.MikroBus2.Pins.RST);

            gps.GnssDataReceived += (s, e) =>
            {
                if (e is GnssPositionInfo info)
                {
                    Console.WriteLine($"Position: {info.Position.Latitude}, {info.Position.Longitude}");
                }
                else if (e is CourseOverGround cog)
                {
                    Console.WriteLine($"True heading: {cog.TrueHeading}");
                }
            };

            gps.StartUpdating();

            Console.WriteLine("Init complete");

            return base.Initialize();
        }

        public async override Task Run()
        {
            Console.WriteLine("Run...");


        }
    }
}