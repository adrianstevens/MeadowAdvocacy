using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.mikroBUS.Sensors.Gnss;
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

        CGNSS5 gps;

        GnssPositionInfo lastPosition;
        ActiveSatellites activeSatellites;
        SatellitesInView satellitesInView;

        MicroGraphics graphics;

        public override Task Run()
        {
            Console.WriteLine("Run...");

            RegisterGPSData();
            gps.StartUpdating();

            projLab.Display.Clear(Color.Red);
            projLab.Display.Show();

            graphics = new MicroGraphics(projLab.Display)
            {
                CurrentFont = new Font12x20(),
                DelayBetweenFrames = TimeSpan.FromSeconds(5)
            };

            return base.Run();
        }

        void RegisterGPSData()
        {
            gps.GgaReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{lastPosition = location}");
                Resolver.Log.Info("*********************************************");
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{lastPosition = location}");
                Resolver.Log.Info("*********************************************");
            };
            // GSA
            gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{this.activeSatellites = activeSatellites}");
                Resolver.Log.Info("*********************************************");
            };
            // RMC (recommended minimum)
            gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{positionCourseAndTime}");
                Resolver.Log.Info("*********************************************");

            };
            // VTG (course made good)
            gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{courseAndVelocity}");
                Resolver.Log.Info("*********************************************");
            };
            // GSV (satellites in view)
            gps.GsvReceived += (object sender, SatellitesInView satellites) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{satellitesInView = satellites}");
                Resolver.Log.Info("*********************************************");

                UpdateDisplay();
            };
        }

        public Task UpdateDisplay()
        {
            graphics.Clear();

            graphics.DrawText(0, 0, "GPS Tracker", color: Color.White);

            if (lastPosition != null)
            {
                graphics.DrawText(0, 40, $"# satellites: {lastPosition.NumberOfSatellites}", color: Color.LawnGreen);
                graphics.DrawText(0, 60, $"Fix quality: {lastPosition.FixQuality}", color: Color.LawnGreen);

                graphics.DrawText(0, 100, $"Lattitude:", color: Color.LawnGreen);
                graphics.DrawText(0, 120, $"{lastPosition.Position.Longitude}", color: Color.LawnGreen);

                graphics.DrawText(0, 160, $"Longitude:", color: Color.LawnGreen);
                graphics.DrawText(0, 180, $"{lastPosition.Position.Latitude}", color: Color.LawnGreen);
            }

            return graphics.ShowBuffered();
        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            IDigitalOutputPort chipSelectPort, resetPort;

            if (projLab is ProjectLabHardwareV2 { } projlabV2)
            {
                chipSelectPort = projlabV2.Mcp_2.CreateDigitalOutputPort(projlabV2.MikroBus1Pins.CS, outputType: OutputType.PushPull);
                resetPort = projlabV2.Mcp_2.CreateDigitalOutputPort(projlabV2.MikroBus1Pins.RST, outputType: OutputType.PushPull);
            }
            else
            {
                chipSelectPort = Device.CreateDigitalOutputPort(projLab.MikroBus1Pins.CS);
                resetPort = null;
            }

            gps = new CGNSS5(Device, Device.PlatformOS.GetSerialPortName("COM1"), Device.Pins.D02, Device.Pins.A03);

            //gps = new CGNSS10(projLab.SpiBus, chipSelectPort, resetPort);

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}