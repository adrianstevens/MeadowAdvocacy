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

            // Alarm indicator dot (top-right)
            if (_alarm.AlarmTime != null)
            {
                graphics.DrawCircle(310, 10, 5, Color.Orange, filled: true);
            }
        }
    }
}
