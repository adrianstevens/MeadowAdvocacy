using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TravelClock.Core.Views;

namespace TravelClock.Core
{
    public class ClockController
    {
        private readonly MicroGraphics _graphics;
        private readonly List<IClockView> _views;
        private readonly AlarmService _alarm;
        private readonly IToneGenerator? _speaker;
        private int _viewIndex;
        private Timer? _timer;
        private bool _alarmFiring;
        private bool _overlayVisible; // flashes each tick

        public AlarmService Alarm => _alarm;

        public ClockController(MicroGraphics graphics, IToneGenerator? speaker = null)
        {
            _graphics = graphics;
            _speaker = speaker;
            _alarm = new AlarmService();
            _alarm.AlarmTriggered += OnAlarmTriggered;

            _views = new List<IClockView>
            {
                new MainClockView(_alarm),
                new CalendarView(),
                new WorldClocksView(WorldCities.All),
                new WorldMapView(WorldCities.All),
                new AlarmSetView(_alarm),
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
            if (_alarmFiring) { DismissAlarm(); return; }

            if (_views[_viewIndex] is IInteractiveView iv && iv.HandleNext())
            {
                RenderNow();
                return;
            }
            _viewIndex = (_viewIndex + 1) % _views.Count;
            RenderNow();
        }

        public void PreviousView()
        {
            if (_alarmFiring) { DismissAlarm(); return; }

            if (_views[_viewIndex] is IInteractiveView iv && iv.HandlePrevious())
            {
                RenderNow();
                return;
            }
            _viewIndex = (_viewIndex - 1 + _views.Count) % _views.Count;
            RenderNow();
        }

        public void UpField()
        {
            if (_alarmFiring) { DismissAlarm(); return; }

            if (_views[_viewIndex] is IInteractiveView iv)
                iv.HandleUp();
            RenderNow();
        }

        public void DownField()
        {
            if (_alarmFiring) { DismissAlarm(); return; }

            if (_views[_viewIndex] is IInteractiveView iv)
                iv.HandleDown();
            RenderNow();
        }

        private void DismissAlarm()
        {
            _alarmFiring = false;
            _overlayVisible = false;
            _alarm.ClearAlarm();
        }

        private void OnAlarmTriggered(object? sender, EventArgs e)
        {
            _alarmFiring = true;
            // Switch to MainClockView (index 0)
            _viewIndex = 0;
            // Play beeps in background
            _ = BeepAsync();
        }

        private async Task BeepAsync()
        {
            if (_speaker == null) return;
            var freq = new Frequency(880, Frequency.UnitType.Hertz);
            for (int i = 0; i < 3; i++)
            {
                await _speaker.PlayTone(freq, TimeSpan.FromMilliseconds(300));
                await Task.Delay(150);
            }
        }

        private void OnTick(object? state)
        {
            var now = DateTime.Now;
            _alarm.Check(now);
            _views[_viewIndex].Render(_graphics, now);

            if (_alarmFiring)
            {
                _overlayVisible = !_overlayVisible;
                if (_overlayVisible)
                    DrawAlarmOverlay(now);
            }

            _graphics.Show();
        }

        private void RenderNow()
        {
            var now = DateTime.Now;
            _views[_viewIndex].Render(_graphics, now);
            if (_alarmFiring && _overlayVisible)
                DrawAlarmOverlay(now);
            _graphics.Show();
        }

        private void DrawAlarmOverlay(DateTime now)
        {
            // Red banner across the middle
            _graphics.DrawRectangle(0, 96, 320, 48, Color.Red, filled: true);
            _graphics.CurrentFont = new Font12x16();
            string label = $"ALARM {now:HH:mm}";
            int w = label.Length * 12;
            _graphics.DrawText((320 - w) / 2, 108, label, Color.White);
        }
    }
}
