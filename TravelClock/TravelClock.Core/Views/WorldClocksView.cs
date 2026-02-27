using Meadow;
using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;

namespace TravelClock.Core.Views
{
    public class WorldClocksView : IClockView
    {
        private readonly IReadOnlyList<WorldCity> _cities;

        public WorldClocksView(IReadOnlyList<WorldCity> cities)
        {
            _cities = cities;
        }

        public void Render(MicroGraphics graphics, DateTime now)
        {
            graphics.Clear();

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(4, 4, "World Clocks", Color.White);
            graphics.DrawLine(0, 22, 319, 22, Color.DarkGray);

            graphics.CurrentFont = new Font8x12();

            int colW = 160;
            int rowH = (240 - 24) / 4;
            DateTime utcNow = now.ToUniversalTime();

            for (int i = 0; i < _cities.Count && i < 8; i++)
            {
                int col = i / 4;
                int row = i % 4;
                int x = col * colW + 4;
                int y = 24 + row * rowH;

                var city = _cities[i];
                var localTime = city.LocalTime(utcNow);

                // City name
                string cityName = city.Name.Length > 11 ? city.Name.Substring(0, 11) : city.Name;
                graphics.DrawText(x, y + 2, cityName, Color.LightGray);

                // Local time
                string timeStr = localTime.ToString("HH:mm");
                graphics.DrawText(x, y + 16, timeStr, Color.White);

                // Row separator
                if (row < 3)
                    graphics.DrawLine(col * colW, y + rowH, col * colW + colW - 4, y + rowH, Color.DarkGray);
            }

            // Column separator
            graphics.DrawLine(160, 24, 160, 239, Color.DarkGray);
        }
    }
}
