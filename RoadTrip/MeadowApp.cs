using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace RoadTrip
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab = default!;
        MicroGraphics graphics = default!;
        ITripSource source = default!;
        TripService trip = default!;
        bool _simMode = true;
        DateTime _rightPressStart;

        // Audi RS palette
        static readonly Color AudiRed = new Color(187 / 255f, 10 / 255f, 33 / 255f);
        static readonly Color Silver  = new Color(200 / 255f, 200 / 255f, 200 / 255f);
        static readonly Color Dim     = new Color(90  / 255f, 90  / 255f, 90  / 255f);

        // Three-column x positions
        const int C1 = 5;
        const int C2 = 112;
        const int C3 = 219;

        public override Task Initialize()
        {
            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display!)
            {
                CurrentFont = new Font12x20()
            };

            trip = new TripService();

            // Boot in sim mode — right button switches to real GPS
            StartSource(new SimulatedTripSource());

            // Left button: reset trip stats
            projLab.LeftButton!.PressStarted += (s, e) =>
            {
                trip = new TripService();
            };

            // Right button: short press = toggle sim/GPS
            projLab.RightButton!.PressStarted += (s, e) => _rightPressStart = DateTime.UtcNow;
            projLab.RightButton!.PressEnded += (s, e) =>
            {
                var held = (DateTime.UtcNow - _rightPressStart).TotalMilliseconds;
                if (held < 700)
                    ToggleSource();
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            DrawAcquiring();
            return Task.CompletedTask;
        }

        void StartSource(ITripSource newSource)
        {
            source?.Stop();
            source = newSource;
            source.ReadingUpdated += OnReadingUpdated;
            source.Start();
        }

        void ToggleSource()
        {
            _simMode = !_simMode;
            trip = new TripService();
            StartSource(_simMode
                ? (ITripSource)new SimulatedTripSource()
                : new TripGpsSource(projLab.MikroBus1));
        }

        void OnReadingUpdated(object? sender, TripReading reading)
        {
            if (reading.HasFix)
                trip.Update(reading);

            Render(reading);

            if (!reading.HasFix)
                projLab.RgbLed?.SetColor(Color.Blue);
            else if (reading.SpeedKph >= 5)
                projLab.RgbLed?.SetColor(AudiRed);
            else
                projLab.RgbLed?.SetColor(Color.White);
        }

        void DrawAcquiring()
        {
            graphics.Clear(Color.Black);
            DrawHeader("SIM", Color.Yellow, sim: true);
            graphics.DrawText(graphics.Width / 2, 110, "Starting...",
                Color.Yellow, alignmentH: HorizontalAlignment.Center);
            graphics.Show();
            projLab.RgbLed?.SetColor(Color.Blue);
        }

        void Render(TripReading r)
        {
            graphics.Clear(Color.Black);

            var fixText  = _simMode ? "SIM" : r.HasFix ? $"FIX({r.SatelliteCount})" : "ACQ...";
            var fixColor = _simMode ? Color.Yellow : r.HasFix ? Color.Green : Color.Yellow;
            DrawHeader(fixText, fixColor, _simMode);

            if (!_simMode && !r.HasFix)
            {
                graphics.DrawText(graphics.Width / 2, 110, "Acquiring GPS fix...",
                    Color.Yellow, alignmentH: HorizontalAlignment.Center);
                graphics.Show();
                return;
            }

            // ── Speed (large, centred) + heading (right, vertically centred in band) ──
            graphics.DrawText(graphics.Width / 2, 28, $"{r.SpeedKph:F0} km/h",
                Color.White, ScaleFactor.X2, HorizontalAlignment.Center);

            if (r.SpeedKph >= 3)
                graphics.DrawText(graphics.Width - 4, 38,
                    HeadingToCardinal(r.HeadingDegrees), AudiRed,
                    alignmentH: HorizontalAlignment.Right);

            graphics.DrawHorizontalLine(0, 84, graphics.Width, Dim);

            // ── Row 1: DIST | AVG | MAX SPD ─────────────────────────────
            graphics.DrawText(C1, 88, "DIST",    Dim);
            graphics.DrawText(C2, 88, "AVG",     Dim);
            graphics.DrawText(C3, 88, "MAX SPD", Dim);

            graphics.DrawText(C1, 108, FormatKm(trip.TotalDistanceKm),         Silver);
            graphics.DrawText(C2, 108, $"{trip.AverageSpeedKph:F0} km/h",      Silver);
            graphics.DrawText(C3, 108, $"{trip.MaxSpeedKph:F0} km/h",          Silver);

            graphics.DrawHorizontalLine(0, 130, graphics.Width, Dim);

            // ── Row 2: ALT | HI | LO ────────────────────────────────────
            graphics.DrawText(C1, 134, "ALT", Dim);
            graphics.DrawText(C2, 134, "HI",  Dim);
            graphics.DrawText(C3, 134, "LO",  Dim);

            graphics.DrawText(C1, 154, $"{r.AltitudeMeters:F0} m",  Silver);
            graphics.DrawText(C2, 154, trip.MaxAltitudeMeters.HasValue
                ? $"{trip.MaxAltitudeMeters.Value:F0} m" : "---", Silver);
            graphics.DrawText(C3, 154, trip.MinAltitudeMeters.HasValue
                ? $"{trip.MinAltitudeMeters.Value:F0} m" : "---", Silver);

            graphics.DrawHorizontalLine(0, 176, graphics.Width, Dim);

            // ── Row 3: MOVING | TOTAL ────────────────────────────────────
            graphics.DrawText(C1, 180, "MOVING", Dim);
            graphics.DrawText(C2, 180, "TOTAL",  Dim);

            graphics.DrawText(C1, 200, FormatTime(trip.MovingTime),  AudiRed);
            graphics.DrawText(C2, 200, FormatTime(trip.SessionTime), Silver);

            graphics.Show();
        }

        void DrawHeader(string fixText, Color fixColor, bool sim)
        {
            graphics.DrawText(graphics.Width / 2, 2, "ROAD TRIP", AudiRed,
                alignmentH: HorizontalAlignment.Center);
            graphics.DrawText(graphics.Width - 4, 2, fixText, fixColor,
                alignmentH: HorizontalAlignment.Right);
            if (sim)
                graphics.DrawText(4, 2, "SIM", Color.Yellow);
            graphics.DrawHorizontalLine(0, 22, graphics.Width, AudiRed);
        }

        static string FormatKm(double km) =>
            km < 10 ? $"{km:F1} km" : $"{km:F0} km";

        static string FormatTime(TimeSpan t) =>
            $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}";

        static string HeadingToCardinal(double heading)
        {
            string[] dirs = { "N","NNE","NE","ENE","E","ESE","SE","SSE",
                               "S","SSW","SW","WSW","W","WNW","NW","NNW" };
            return dirs[(int)(Math.Round(heading / 22.5) % 16)];
        }
    }
}
