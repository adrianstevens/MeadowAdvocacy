using System;
using System.Collections.Generic;

namespace TravelClock.Core
{
    public static class WorldCities
    {
        public static readonly IReadOnlyList<WorldCity> All = new List<WorldCity>
        {
            new WorldCity("London",      TimeSpan.FromHours( 0), 160, 55),
            new WorldCity("Paris",       TimeSpan.FromHours( 1), 163, 58),
            new WorldCity("Dubai",       TimeSpan.FromHours( 4), 209, 88),
            new WorldCity("Tokyo",       TimeSpan.FromHours( 9), 272, 72),
            new WorldCity("Sydney",      TimeSpan.FromHours(10), 278, 158),
            new WorldCity("Los Angeles", TimeSpan.FromHours(-8),  55, 78),
            new WorldCity("New York",    TimeSpan.FromHours(-5),  94, 68),
            new WorldCity("Sao Paulo",   TimeSpan.FromHours(-3), 119, 148),
        };
    }
}
