using Meadow;
using Meadow.Foundation.Graphics;
using System;

namespace TravelClock.Core.Views
{
    public class MainClockView : IClockView
    {
        private readonly AlarmService _alarm;

        public MainClockView(AlarmService alarm)
        {
            _alarm = alarm;
        }

        public void Render(MicroGraphics graphics, DateTime now)
        {
            graphics.Clear();

            // Large time — Font12x16 at X3 = 36x48 per char
            string timeStr = now.ToString("HH:mm:ss");
            graphics.CurrentFont = new Font12x16();
            int timeWidth = timeStr.Length * 12 * 3;
            int timeX = (320 - timeWidth) / 2;
            graphics.DrawText(timeX, 70, timeStr, Color.White, ScaleFactor.X3);

            // Date line
            string dateStr = now.ToString("ddd MMM dd");
            graphics.CurrentFont = new Font8x12();
            int dateWidth = dateStr.Length * 8;
            int dateX = (320 - dateWidth) / 2;
            graphics.DrawText(dateX, 150, dateStr, Color.LightGray);

            // Alarm indicator — dot + time string (top-right)
            if (_alarm.AlarmTime != null)
            {
                graphics.DrawCircle(10, 10, 5, Color.Orange, filled: true);
                string alarmStr = $"alarm {_alarm.AlarmTime.Value.Hours:D2}:{_alarm.AlarmTime.Value.Minutes:D2}";
                graphics.CurrentFont = new Font8x12();
                int alarmW = alarmStr.Length * 8;
                graphics.DrawText(320 - alarmW - 4, 4, alarmStr, Color.Orange);
            }
        }
    }
}
