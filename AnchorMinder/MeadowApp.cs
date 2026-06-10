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
        bool _alarmAcknowledged = false;
        DateTime _rightPressStart;

        static readonly Color ColorTitle    = new Color(0f,       124/255f, 119/255f); // 0x007C77
        static readonly Color ColorHolding  = new Color(97/255f,  231/255f, 134/255f); // 0x61E786
        static readonly Color ColorNoAnchor = new Color(72/255f,  67/255f,  92/255f);  // 0x48435C

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

            // Right button: short press = toggle drift, long press = acknowledge alarm
            projLab.RightButton!.PressStarted += (s, e) =>
            {
                _rightPressStart = DateTime.UtcNow;
            };
            projLab.RightButton!.PressEnded += (s, e) =>
            {
                var held = (DateTime.UtcNow - _rightPressStart).TotalMilliseconds;
                if (held >= 700 && _alarmActive && !_alarmAcknowledged)
                {
                    _alarmAcknowledged = true;
                    projLab.Speaker?.StopTone();
                    UpdateDisplay();
                }
                else if (held < 700)
                {
                    gps.ToggleDrift();
                    PlayDriftToggle();
                    UpdateDisplay();
                }
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
                _alarmAcknowledged = false;
                projLab.RgbLed?.SetColor(Color.Red);
                _ = SoundAlarm();
            }
            else if (!anchor.IsDragging)
            {
                _alarmActive = false;
                _alarmAcknowledged = false;
                projLab.RgbLed?.SetColor(anchor.IsAnchored ? ColorHolding : ColorNoAnchor);
                projLab.Speaker?.StopTone();
            }
        }

        async Task SoundAlarm()
        {
            while (_alarmActive && !_alarmAcknowledged)
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
            graphics.DrawText(graphics.Width / 2, 2, "ANCHOR MINDER", ColorTitle, alignmentH: HorizontalAlignment.Center);
            graphics.DrawHorizontalLine(0, 24, graphics.Width, ColorTitle);
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
            var statusColor = !anchor.IsAnchored ? ColorNoAnchor
                            : isDragging ? Color.Red
                            : ColorHolding;
            var statusText = !anchor.IsAnchored ? "NO ANCHOR"
                           : isDragging ? "DRAGGING!"
                           : "HOLDING";

            graphics.Clear(Color.Black);

            // Title + divider
            graphics.DrawText(graphics.Width / 2, 2, "ANCHOR MINDER", ColorTitle, alignmentH: HorizontalAlignment.Center);
            graphics.DrawHorizontalLine(0, 24, graphics.Width, ColorTitle);

            // GPS fix + sim indicator (top right)
            var fixText = gps.HasFix ? $"FIX({gps.SatelliteCount})" : "ACQ...";
            var fixColor = gps.HasFix ? ColorHolding : Color.Yellow;
            graphics.DrawText(graphics.Width - 2, 2, fixText, fixColor, alignmentH: HorizontalAlignment.Right);
            if (gps.IsDrifting)
                graphics.DrawText(graphics.Width - 2, 14, "SIM", Color.Orange, alignmentH: HorizontalAlignment.Right);

            // Status — full width
            graphics.DrawText(graphics.Width / 2, 28, statusText, statusColor, ScaleFactor.X2, HorizontalAlignment.Center);

            if (anchor.IsAnchored)
            {
                // Left column: distance + bearing text
                graphics.DrawText(5, 78, $"{anchor.DistanceMetres:F1} m", Color.White, ScaleFactor.X2);
                graphics.DrawText(5, 125, $"Bearing {anchor.BearingDegrees:F0}°", Color.Yellow);

                // Alarm state hint
                if (_alarmActive && _alarmAcknowledged)
                    graphics.DrawText(5, 143, "SILENCED", Color.Orange);
                else if (_alarmActive)
                    graphics.DrawText(5, 143, "HOLD RIGHT TO SILENCE", Color.DarkGray);

                // Right column: radar circle centered at (260, 127), radius 52
                DrawRadar(260, 120, 52);

                // Bottom rows — full width
                graphics.DrawText(5, 155, $"Radius: {anchor.AnchorRadiusMetres:F0} m", Color.DarkGray);

                var ap = anchor.AnchorPosition!;
                graphics.DrawText(5, 178, $"Anchor: {ap.Latitude:F4},{ap.Longitude:F4}", Color.DarkGray);
            }
            else
            {
                if (gps.HasFix)
                    graphics.DrawText(graphics.Width / 2, 110, "LEFT = Drop Anchor", Color.DarkGray, ScaleFactor.X1, HorizontalAlignment.Center);
                else
                    graphics.DrawText(graphics.Width / 2, 110, "Acquiring GPS fix...", Color.Yellow, ScaleFactor.X1, HorizontalAlignment.Center);
                graphics.DrawText(5, 155, $"Radius: {anchor.AnchorRadiusMetres:F0} m", Color.DarkGray);
            }

            // Current position — always at bottom
            graphics.DrawText(5, 200, $"Pos:    {pos.Latitude:F4},{pos.Longitude:F4}", Color.DarkGray);

            graphics.Show();
        }

        void DrawRadar(int centerX, int centerY, int screenRadius)
        {
            var accentColor = anchor.IsDragging ? Color.Red : ColorHolding;

            // Anchor radius boundary circle
            graphics.DrawCircle(centerX, centerY, screenRadius, Color.DarkGray);

            // Anchor icon at center
            DrawAnchorIcon(centerX, centerY, Color.White);

            // Map boat position onto radar
            var bearingRad = anchor.BearingDegrees * Math.PI / 180.0;
            var distRatio = Math.Min(anchor.DistanceMetres / anchor.AnchorRadiusMetres, 1.4);

            var boatX = centerX + (int)(Math.Sin(bearingRad) * distRatio * screenRadius);
            var boatY = centerY - (int)(Math.Cos(bearingRad) * distRatio * screenRadius);

            graphics.DrawLine(centerX, centerY, boatX, boatY, accentColor);

            // Boat icon — triangle pointing in bearing direction
            DrawBoatIcon(boatX, boatY, bearingRad, accentColor);
        }

        void DrawAnchorIcon(int x, int y, Color color)
        {
            // Ring at top
            graphics.DrawCircle(x, y - 7, 3, color);
            // Shank (vertical line)
            graphics.DrawLine(x, y - 4, x, y + 5, color);
            // Stock (horizontal crossbar)
            graphics.DrawLine(x - 5, y - 2, x + 5, y - 2, color);
            // Flukes
            graphics.DrawLine(x, y + 5, x - 4, y + 2, color);
            graphics.DrawLine(x, y + 5, x + 4, y + 2, color);
        }

        void DrawBoatIcon(int x, int y, double bearingRad, Color color)
        {
            // Tip — forward in bearing direction
            var tipX = x + (int)(Math.Sin(bearingRad) * 7);
            var tipY = y - (int)(Math.Cos(bearingRad) * 7);
            // Rear corners — 135° off bearing
            var leftRad = bearingRad + 2.356;
            var rightRad = bearingRad - 2.356;
            var rearX1 = x + (int)(Math.Sin(leftRad) * 5);
            var rearY1 = y - (int)(Math.Cos(leftRad) * 5);
            var rearX2 = x + (int)(Math.Sin(rightRad) * 5);
            var rearY2 = y - (int)(Math.Cos(rightRad) * 5);

            graphics.DrawTriangle(tipX, tipY, rearX1, rearY1, rearX2, rearY2, color, true);
        }
    }
}
