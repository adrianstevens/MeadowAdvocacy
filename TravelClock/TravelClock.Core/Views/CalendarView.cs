using Meadow;
using Meadow.Foundation.Graphics;
using System;

namespace TravelClock.Core.Views
{
    public class CalendarView : IClockView
    {
        public void Render(MicroGraphics graphics, DateTime now)
        {
            graphics.Clear();

            // Header
            graphics.CurrentFont = new Font12x16();
            string header = now.ToString("MMMM yyyy");
            int headerWidth = header.Length * 12;
            graphics.DrawText((320 - headerWidth) / 2, 4, header, Color.White);

            // Day-of-week headers: Su Mo Tu We Th Fr Sa
            graphics.CurrentFont = new Font8x12();
            string[] dayNames = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };
            int cellW = 320 / 7;
            for (int d = 0; d < 7; d++)
            {
                graphics.DrawText(d * cellW + 2, 28, dayNames[d], Color.LightGray);
            }

            // Separator
            graphics.DrawLine(0, 42, 319, 42, Color.DarkGray);

            // Grid
            var firstDay = new DateTime(now.Year, now.Month, 1);
            int startCol = (int)firstDay.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
            int cellH = (240 - 44) / 6;
            Color accentColor = new Color(0x1E, 0x90, 0xFF); // DodgerBlue

            for (int day = 1; day <= daysInMonth; day++)
            {
                int slot = startCol + day - 1;
                int row = slot / 7;
                int col = slot % 7;
                int x = col * cellW;
                int y = 44 + row * cellH;

                if (day == now.Day)
                {
                    graphics.DrawRectangle(x, y, cellW, cellH, accentColor, filled: true);
                    graphics.DrawText(x + 2, y + 2, day.ToString(), Color.Black);
                }
                else
                {
                    graphics.DrawText(x + 2, y + 2, day.ToString(), Color.White);
                }
            }
        }
    }
}
