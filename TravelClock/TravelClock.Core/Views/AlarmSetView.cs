using Meadow;
using Meadow.Foundation.Graphics;
using System;

namespace TravelClock.Core.Views
{
    public class AlarmSetView : IInteractiveView
    {
        private readonly AlarmService _alarm;
        private int _hours = 7;
        private int _minutes = 0;
        private bool _editingMinutes;
        private bool _enabled;

        public AlarmSetView(AlarmService alarm)
        {
            _alarm = alarm;
        }

        private void SyncFromAlarm()
        {
            if (_alarm.AlarmTime.HasValue)
            {
                _hours = _alarm.AlarmTime.Value.Hours;
                _minutes = _alarm.AlarmTime.Value.Minutes;
                _enabled = true;
            }
            else
            {
                _enabled = false;
            }
        }

        private void Save()
        {
            if (_enabled)
                _alarm.SetAlarm(new TimeSpan(_hours, _minutes, 0));
            else
                _alarm.ClearAlarm();
        }

        // Right: HH→MM (consumed), MM→pass through (navigate to next view)
        public bool HandleNext()
        {
            if (!_editingMinutes)
            {
                _editingMinutes = true;
                return true;
            }
            return false;
        }

        // Left: MM→HH (consumed), HH→pass through (navigate to previous view)
        public bool HandlePrevious()
        {
            if (_editingMinutes)
            {
                _editingMinutes = false;
                return true;
            }
            return false;
        }

        public void HandleUp()
        {
            _enabled = true;
            if (_editingMinutes)
                _minutes = (_minutes + 1) % 60;
            else
                _hours = (_hours + 1) % 24;
            Save();
        }

        public void HandleDown()
        {
            _enabled = true;
            if (_editingMinutes)
                _minutes = (_minutes - 1 + 60) % 60;
            else
                _hours = (_hours - 1 + 24) % 24;
            Save();
        }

        public void Render(MicroGraphics graphics, DateTime now)
        {
            SyncFromAlarm();

            graphics.Clear();

            // Header
            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(4, 4, "Set Alarm", Color.White);
            graphics.DrawLine(0, 22, 319, 22, Color.DarkGray);

            if (_enabled)
            {
                // HH : MM  — Font12x16 @ X3 = 36px wide per char, 48px tall
                // "HH:MM" = 5 chars × 36 = 180px; centre at x=70
                int timeY = 88;
                int x = 70;

                Color hourColor = !_editingMinutes ? Color.Yellow : Color.White;
                Color minColor  = _editingMinutes  ? Color.Yellow : Color.White;

                graphics.CurrentFont = new Font12x16();
                graphics.DrawText(x,        timeY, _hours.ToString("D2"),   hourColor, ScaleFactor.X3);
                graphics.DrawText(x + 72,   timeY, ":",                      Color.White, ScaleFactor.X3);
                graphics.DrawText(x + 108,  timeY, _minutes.ToString("D2"), minColor,  ScaleFactor.X3);

                // Cursor underline under selected field
                int cursorX = _editingMinutes ? x + 108 : x;
                graphics.DrawLine(cursorX, timeY + 50, cursorX + 71, timeY + 50, Color.Yellow);

                // Hint
                graphics.CurrentFont = new Font8x12();
                string hint = _editingMinutes ? "< hours   up/dn min   > done" : "up/dn hrs   > minutes";
                graphics.DrawText(4, 210, hint, Color.DarkGray);
            }
            else
            {
                // No alarm set
                graphics.CurrentFont = new Font12x16();
                graphics.DrawText(70, 88, "--:--", Color.DarkGray, ScaleFactor.X3);

                graphics.CurrentFont = new Font8x12();
                graphics.DrawText(4, 210, "up/dn to set alarm", Color.DarkGray);
            }

            // Status line
            graphics.CurrentFont = new Font8x12();
            string status = _enabled
                ? $"Alarm: {_hours:D2}:{_minutes:D2}"
                : "Alarm: OFF";
            int statusW = status.Length * 8;
            graphics.DrawText((320 - statusW) / 2, 170, status,
                _enabled ? Color.Orange : Color.DarkGray);
        }
    }
}
