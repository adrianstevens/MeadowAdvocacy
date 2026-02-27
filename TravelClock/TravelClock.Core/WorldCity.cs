using System;

namespace TravelClock.Core
{
    public class WorldCity
    {
        public string Name { get; }
        public TimeSpan UtcOffset { get; }
        public int MapX { get; }
        public int MapY { get; }

        public WorldCity(string name, TimeSpan utcOffset, int mapX, int mapY)
        {
            Name = name;
            UtcOffset = utcOffset;
            MapX = mapX;
            MapY = mapY;
        }

        public DateTime LocalTime(DateTime utcNow) => utcNow + UtcOffset;
    }
}
