using Meadow.Foundation.Graphics;
using System;

namespace TravelClock.Core.Views
{
    public interface IClockView
    {
        void Render(MicroGraphics graphics, DateTime now);
    }
}
