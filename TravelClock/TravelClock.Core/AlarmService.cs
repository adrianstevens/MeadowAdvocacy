using System;

namespace TravelClock.Core
{
    public class AlarmService
    {
        public TimeSpan? AlarmTime { get; private set; }

        public event EventHandler? AlarmTriggered;

        private bool _triggered;

        public void SetAlarm(TimeSpan timeOfDay)
        {
            AlarmTime = timeOfDay;
            _triggered = false;
        }

        public void ClearAlarm()
        {
            AlarmTime = null;
            _triggered = false;
        }

        public void Check(DateTime now)
        {
            if (AlarmTime == null || _triggered)
                return;

            var todayAlarm = now.Date + AlarmTime.Value;
            if (now >= todayAlarm && now < todayAlarm.AddMinutes(1))
            {
                _triggered = true;
                AlarmTriggered?.Invoke(this, EventArgs.Empty);
            }
            else if (now < todayAlarm)
            {
                _triggered = false;
            }
        }
    }
}
