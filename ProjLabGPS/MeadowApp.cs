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
            graphics.CurrentFont = new Font12x20();

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
            //GGA
            gps.GgaReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("GGA *********************************************");
                Resolver.Log.Info($"{lastPosition = location}");

                UpdateDisplay("GGA");
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("GLL *********************************************");
                Resolver.Log.Info($"{lastPosition = location}");

                UpdateDisplay("GLL");
            };
            // GSA
            gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                Resolver.Log.Info("GSA *********************************************");
                Resolver.Log.Info($"{this.activeSatellites = activeSatellites}");
                Resolver.Log.Info("GSA *********************************************");

                UpdateDisplay("GSA");
            };
            // RMC (recommended minimum)
            gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                Resolver.Log.Info("RMC *********************************************");
                Resolver.Log.Info($"{positionCourseAndTime}");

                UpdateDisplay("RMC");
            };
            // VTG (course made good)
            gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                Resolver.Log.Info("VTG *********************************************");
                Resolver.Log.Info($"{courseAndVelocity}");

                UpdateDisplay("VTG");
            };
            // GSV (satelliteCount in view)
            gps.GsvReceived += (object sender, SatellitesInView satellites) =>
            {
                Resolver.Log.Info("GSV *********************************************");
                Resolver.Log.Info($"{satellitesInView = satellites}");

                UpdateDisplay("GSV");
            };
        }

        public Task UpdateDisplay(string signalType)
        {
            graphics.Clear();

            graphics.DrawText(0, 0, "GPS Tester", color: Color.White);

            graphics.DrawText(graphics.Width, 0, signalType, color: Color.Blue, alignmentH: HorizontalAlignment.Right);

            var hasFix = lastPosition?.Position is not null;

            if (!hasFix)
            {
                graphics.DrawText(0, 20, "No GPS signal", color: Color.Red);
                return graphics.ShowBuffered();
            }

            syncTime ??= DateTime.Now;

            var elapsedSeconds = (int)Math.Round((syncTime! - startTime).Value.TotalSeconds);
            graphics.DrawText(0, 20, $"Synced in {elapsedSeconds}s", color: Color.LawnGreen);

            var satelliteCount = activeSatellites.SatellitesUsedForFix?.Length.ToString() ?? "N/A";
            var satellitesList = activeSatellites.SatellitesUsedForFix is not null &&
                activeSatellites.SatellitesUsedForFix.Length > 0
                ? string.Join(",", activeSatellites.SatellitesUsedForFix) : "N/A";

            var quality = lastPosition?.FixQuality?.ToString() ?? "N/A";
            var altitude = $"{lastPosition?.Position?.Altitude.Meters:F1} m" ?? "N/A";
            var latitude = $"{lastPosition?.Position?.Latitude:F6}°" ?? "N/A";
            var longitude = $"{lastPosition?.Position?.Longitude:F6}°" ?? "N/A";

            graphics.DrawText(0, 60, $"# satellites: {satelliteCount}", color: Color.Yellow);
            graphics.DrawText(0, 80, $"{satellitesList}", color: Color.Yellow);
            graphics.DrawText(0, 100, $"Fix quality: {quality}", color: Color.Yellow);

            graphics.DrawText(0, 140, $"Altitude:    {altitude}", color: Color.Yellow);
            graphics.DrawText(0, 160, $"Latitude:    {latitude}", color: Color.Yellow);
            graphics.DrawText(0, 180, $"Longitude:   {longitude}", color: Color.Yellow);

            return graphics.ShowBuffered();
        }
    }
}