using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.mikroBUS.Sensors.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Threading.Tasks;

namespace ProjectLabGPS
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab = default!;
        CGNSS10 gps = default!;

        GnssPositionInfo? lastPosition;
        ActiveSatellites activeSatellites;
        SatellitesInView satellitesInView;

        DateTime startTime;
        DateTime? syncTime;

        MicroGraphics graphics = default!;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            startTime = DateTime.Now;

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display!);

            gps = new CGNSS10(projLab.MikroBus2.SpiBus, projLab.MikroBus2.Pins.CS, projLab.MikroBus2.Pins.RST);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            RegisterGPSData();
            gps.StartUpdating();

            graphics.Clear(Color.AliceBlue, true);


            Console.WriteLine("Waiting for up button press");

            while (true)
            {
                await Task.Delay(10);
            }
        }

        void RegisterGPSData()
        {
            gps.GgaReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{lastPosition = location}");
                Resolver.Log.Info("*********************************************");

                UpdateDisplay();
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{lastPosition = location}");
                Resolver.Log.Info("*********************************************");

                UpdateDisplay();
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

                UpdateDisplay();
            };
            // VTG (course made good)
            gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{courseAndVelocity}");
                Resolver.Log.Info("*********************************************");

                UpdateDisplay();
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
            syncTime ??= DateTime.Now;

            graphics.Clear();

            graphics.DrawText(0, 0, $"GPS Tracker {syncTime - startTime}", color: Color.White);

            if (lastPosition != null)
            {
                graphics.DrawText(0, 40, $"# satellites: {lastPosition?.NumberOfSatellites}", color: Color.LawnGreen);
                graphics.DrawText(0, 60, $"Fix quality: {lastPosition?.FixQuality}", color: Color.LawnGreen);

                graphics.DrawText(0, 100, $"Lattitude:", color: Color.LawnGreen);
                graphics.DrawText(0, 120, $"{lastPosition?.Position!.Longitude}", color: Color.LawnGreen);

                graphics.DrawText(0, 160, $"Longitude:", color: Color.LawnGreen);
                graphics.DrawText(0, 180, $"{lastPosition?.Position!.Latitude}", color: Color.LawnGreen);
            }

            return graphics.ShowBuffered();
        }
    }
}