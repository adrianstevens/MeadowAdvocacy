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
            graphics.DrawText(graphics.Width / 2, 2, "ANCHOR MINDER", Color.Navy, alignmentH: HorizontalAlignment.Center);
            graphics.DrawHorizontalLine(0, 24, graphics.Width, Color.Navy);
            graphics.DrawText(graphics.Width / 2, 100, "Acquiring position...", Color.Gray, alignmentH: HorizontalAlignment.Center);
            graphics.DrawText(graphics.Width / 2, 130, "LEFT = Drop Anchor", Color.DarkGray, ScaleFactor.X1, HorizontalAlignment.Center);
            graphics.DrawText(graphics.Width / 2, 150, "RIGHT = Simulate Drift", Color.DarkGray, ScaleFactor.X1, HorizontalAlignment.Center);
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

            // Title + divider
            graphics.DrawText(graphics.Width / 2, 2, "ANCHOR MINDER", Color.Navy, alignmentH: HorizontalAlignment.Center);
            graphics.DrawHorizontalLine(0, 24, graphics.Width, Color.Navy);

            // Sim indicator (top right)
            if (gps.IsDrifting)
                graphics.DrawText(graphics.Width - 2, 2, "SIM", Color.Orange, alignmentH: HorizontalAlignment.Right);

            // Status — full width
            graphics.DrawText(graphics.Width / 2, 28, statusText, statusColor, ScaleFactor.X2, HorizontalAlignment.Center);

            if (anchor.IsAnchored)
            {
                // Left column: distance + bearing text
                graphics.DrawText(5, 78, $"{anchor.DistanceMetres:F1} m", Color.White, ScaleFactor.X2);
                graphics.DrawText(5, 125, $"Bearing {anchor.BearingDegrees:F0}°", Color.Yellow);

                // Right column: radar circle centered at (260, 127), radius 52
                DrawRadar(260, 120, 52);

                // Bottom rows — full width
                graphics.DrawText(5, 155, $"Radius: {anchor.AnchorRadiusMetres:F0} m", Color.DarkGray);

                var ap = anchor.AnchorPosition!;
                graphics.DrawText(5, 178, $"Anchor: {ap.Latitude:F4},{ap.Longitude:F4}", Color.DarkGray);
            }
            else
            {
                graphics.DrawText(graphics.Width / 2, 110, "LEFT = Drop Anchor", Color.DarkGray, ScaleFactor.X1, HorizontalAlignment.Center);
                graphics.DrawText(5, 155, $"Radius: {anchor.AnchorRadiusMetres:F0} m", Color.DarkGray);
            }

            // Current position — always at bottom
            graphics.DrawText(5, 200, $"Pos:    {pos.Latitude:F4},{pos.Longitude:F4}", Color.DarkGray);

            graphics.Show();
        }

        void DrawRadar(int centerX, int centerY, int screenRadius)
        {
            var accentColor = anchor.IsDragging ? Color.Red : Color.Green;

            // Anchor radius boundary circle
            graphics.DrawCircle(centerX, centerY, screenRadius, Color.DarkGray);

            // Anchor point
            graphics.DrawCircle(centerX, centerY, 3, Color.White, true);

            // Map boat position onto radar
            var bearingRad = anchor.BearingDegrees * Math.PI / 180.0;
            var distRatio = Math.Min(anchor.DistanceMetres / anchor.AnchorRadiusMetres, 1.4);

            var boatX = centerX + (int)(Math.Sin(bearingRad) * distRatio * screenRadius);
            var boatY = centerY - (int)(Math.Cos(bearingRad) * distRatio * screenRadius);

            graphics.DrawLine(centerX, centerY, boatX, boatY, accentColor);
            graphics.DrawCircle(boatX, boatY, 5, accentColor, true);
        }
    }
}
