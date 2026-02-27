using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Collections.Generic;

namespace TravelClock.Core.Views
{
    public class WorldMapView : IClockView
    {
        private readonly IReadOnlyList<WorldCity> _cities;
        private Image? _mapImage;

        public WorldMapView(IReadOnlyList<WorldCity> cities)
        {
            _cities = cities;
            TryLoadMap();
        }

        private void TryLoadMap()
        {
            try
            {
                _mapImage = Image.LoadFromResource("worldmap.bmp");
            }
            catch
            {
                _mapImage = null;
            }
        }

        public void Render(MicroGraphics graphics, DateTime now)
        {
            graphics.Clear();

            if (_mapImage?.DisplayBuffer != null)
            {
                graphics.DrawBuffer(0, 0, _mapImage.DisplayBuffer);
            }
            else
            {
                // Fallback: dark background with simple grid
                graphics.DrawRectangle(0, 0, 320, 240, new Color(0x1a, 0x1a, 0x3a), filled: true);
                graphics.DrawText(4, 4, "World Map", Color.White);
            }

            graphics.CurrentFont = new Font4x8();
            DateTime utcNow = now.ToUniversalTime();

            foreach (var city in _cities)
            {
                var localTime = city.LocalTime(utcNow);
                string timeLabel = localTime.ToString("HH:mm");

                // City dot
                graphics.DrawCircle(city.MapX, city.MapY, 3, Color.Red, filled: true);

                // Time label — offset to avoid the dot
                int labelX = city.MapX + 5;
                int labelY = city.MapY - 4;
                if (labelX + timeLabel.Length * 4 > 319) labelX = city.MapX - timeLabel.Length * 4 - 2;
                if (labelY < 0) labelY = city.MapY + 5;

                graphics.DrawText(labelX, labelY, timeLabel, Color.Yellow);
            }
        }
    }
}
