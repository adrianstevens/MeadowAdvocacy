using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;
using TravelClock.Core.Views;

namespace TravelClock.Core
{
    public class ClockController
    {
        private readonly MicroGraphics _graphics;
        private readonly List<IClockView> _views;
        private readonly AlarmService _alarm;
        private int _viewIndex;
        private Timer? _timer;

        public AlarmService Alarm => _alarm;

        public ClockController(MicroGraphics graphics)
        {
            _graphics = graphics;
            _alarm = new AlarmService();

            _views = new List<IClockView>
            {
                new MainClockView(_alarm),
                new CalendarView(),
                new WorldClocksView(WorldCities.All),
                new WorldMapView(WorldCities.All),
            };
        }

        public void Start()
        {
            _timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void NextView()
        {
            _viewIndex = (_viewIndex + 1) % _views.Count;
            RenderNow();
        }

        public void PreviousView()
        {
            _viewIndex = (_viewIndex - 1 + _views.Count) % _views.Count;
            RenderNow();
        }

        private void OnTick(object? state)
        {
            var now = DateTime.Now;
            _alarm.Check(now);
            _views[_viewIndex].Render(_graphics, now);
            _graphics.Show();
        }

        private void RenderNow()
        {
            var now = DateTime.Now;
            _views[_viewIndex].Render(_graphics, now);
            _graphics.Show();
        }
    }
}
