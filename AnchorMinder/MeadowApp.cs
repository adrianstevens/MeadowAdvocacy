using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace AnchorMinder
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projLab = default!;
        MicroGraphics graphics = default!;
        SimulatedGpsSource gps = default!;
        AnchorService anchor = default!;

        bool _alarmActive = false;

        public override Task Initialize()
        {
            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display!)
            {
                CurrentFont = new Font12x20()
            };

            anchor = new AnchorService { AnchorRadiusMetres = 25 };
            anchor.StateChanged += (s, e) => UpdateDisplay();

            gps = new SimulatedGpsSource();
            gps.PositionUpdated += OnPositionUpdated;

            // Left button: drop / weigh anchor
            projLab.LeftButton!.PressStarted += (s, e) =>
            {
                if (anchor.IsAnchored)
                {
                    anchor.WeighAnchor();
                    PlayWeighAnchor();
                }
                else if (anchor.CurrentPosition is not null)
                {
                    anchor.DropAnchor(anchor.CurrentPosition);
                    PlayDropAnchor();
                }
            };

            // Right button: toggle simulated drift
            projLab.RightButton!.PressStarted += (s, e) =>
            {
                gps.ToggleDrift();
                PlayDriftToggle();
                UpdateDisplay();
            };

            // Up button: increase radius by 5m
            projLab.UpButton!.PressStarted += (s, e) =>
            {
                anchor.AnchorRadiusMetres += 5;
                PlayRadiusAdjust();
                UpdateDisplay();
            };

            // Down button: decrease radius (min 5m)
            projLab.DownButton!.PressStarted += (s, e) =>
            {
                anchor.AnchorRadiusMetres = Math.Max(5, anchor.AnchorRadiusMetres - 5);
                PlayRadiusAdjust();
                UpdateDisplay();
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            gps.Start();
            DrawWaitingScreen();
            return Task.CompletedTask;
        }

        void OnPositionUpdated(object? sender, GeoPosition position)
        {
            anchor.UpdatePosition(position);
            UpdateAlarm();
        }

        void UpdateAlarm()
        {
            if (anchor.IsDragging && !_alarmActive)
            {
                _alarmActive = true;
                projLab.RgbLed?.SetColor(Color.Red);
                _ = SoundAlarm();
            }
            else if (!anchor.IsDragging)
            {
                _alarmActive = false;
                projLab.RgbLed?.SetColor(anchor.IsAnchored ? Color.Green : Color.Blue);
                projLab.Speaker?.StopTone();
            }
        }

        async Task SoundAlarm()
        {
            while (_alarmActive)
            {
                projLab.Speaker?.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(300));
                await Task.Delay(600);
                if (!_alarmActive) break;
                projLab.Speaker?.PlayTone(new Frequency(660), TimeSpan.FromMilliseconds(300));
                await Task.Delay(600);
            }
        }

        void PlayDropAnchor()
        {
            _ = Task.Run(() =>
            {
                projLab.Speaker?.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(120));
                projLab.Speaker?.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(220));
            });
        }

        void PlayWeighAnchor()
        {
            _ = Task.Run(() =>
            {
                projLab.Speaker?.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(120));
                projLab.Speaker?.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(220));
            });
        }

        void PlayDriftToggle()
        {
            _ = Task.Run(() =>
            {
                projLab.Speaker?.PlayTone(new Frequency(660), TimeSpan.FromMilliseconds(80));
            });
        }

        void PlayRadiusAdjust()
        {
            _ = Task.Run(() =>
            {
                projLab.Speaker?.PlayTone(new Frequency(1100), TimeSpan.FromMilliseconds(40));
            });
        }

        void DrawWaitingScreen()
        {
            graphics.Clear(Color.Black);
            graphics.DrawText(graphics.Width / 2, 10, "ANCHOR MINDER", Color.Cyan, alignmentH: HorizontalAlignment.Center);
            graphics.DrawText(graphics.Width / 2, 100, "Acquiring position...", Color.Gray, alignmentH: HorizontalAlignment.Center);
            graphics.DrawText(graphics.Width / 2, 200, "LEFT = Drop/Weigh Anchor", Color.DarkGray, ScaleFactor.X1, HorizontalAlignment.Center);
            graphics.DrawText(graphics.Width / 2, 220, "RIGHT = Simulate Drift", Color.DarkGray, ScaleFactor.X1, HorizontalAlignment.Center);
            graphics.Show();

            projLab.RgbLed?.SetColor(Color.Blue);
        }

        void UpdateDisplay()
        {
            var pos = anchor.CurrentPosition;
            if (pos is null) return;

            var isDragging = anchor.IsDragging;
            var statusColor = !anchor.IsAnchored ? Color.Blue
                            : isDragging ? Color.Red
                            : Color.Green;
            var statusText = !anchor.IsAnchored ? "NO ANCHOR"
                           : isDragging ? "DRAGGING!"
                           : "HOLDING";

            graphics.Clear(Color.Black);

            // Title
            graphics.DrawText(graphics.Width / 2, 5, "ANCHOR MINDER", Color.Cyan, alignmentH: HorizontalAlignment.Center);
            graphics.DrawHorizontalLine(0, 28, graphics.Width, Color.DarkCyan);

            // Status
            graphics.DrawText(graphics.Width / 2, 38, statusText, statusColor, ScaleFactor.X2, HorizontalAlignment.Center);

            if (anchor.IsAnchored)
            {
                // Distance
                graphics.DrawText(graphics.Width / 2, 85, $"{anchor.DistanceMetres:F1} m", Color.White, ScaleFactor.X2, HorizontalAlignment.Center);

                // Bearing
                graphics.DrawText(graphics.Width / 2, 130, $"Bearing {anchor.BearingDegrees:F0}°", Color.Yellow, alignmentH: HorizontalAlignment.Center);

                // Radius
                graphics.DrawText(graphics.Width / 2, 155, $"Radius: {anchor.AnchorRadiusMetres:F0}m", Color.DarkGray, ScaleFactor.X1, HorizontalAlignment.Center);

                // Anchor position
                var ap = anchor.AnchorPosition!;
                graphics.DrawText(5, 180, $"A: {ap.Latitude:F4}, {ap.Longitude:F4}", Color.DarkGray);
            }

            // Current position
            graphics.DrawText(5, 200, $"P: {pos.Latitude:F4}, {pos.Longitude:F4}", Color.DarkGray);

            // Sim indicator
            if (gps.IsDrifting)
                graphics.DrawText(graphics.Width - 5, 5, "SIM DRIFT", Color.Orange, alignmentH: HorizontalAlignment.Right);

            graphics.Show();
        }
    }
}
