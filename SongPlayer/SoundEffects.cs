using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace SongPlayer
{
    public class SoundEffects
    {
        /// <summary>
        /// Represents the different sound effects that can be played by the <see cref="PiezoSoundPlayer"/> class.
        /// </summary>
        public enum Effect
        {
            /// <summary>
            /// A simple beep sound effect.
            /// </summary>
            Beep,
            /// <summary>
            /// A success or positive feedback sound effect.
            /// </summary>
            Success,
            /// <summary>
            /// A failure or error sound effect.
            /// </summary>
            Failure,
            /// <summary>
            /// A warning or caution sound effect.
            /// </summary>
            Warning,
            /// <summary>
            /// An alarm or emergency sound effect.
            /// </summary>
            Alarm,
            /// <summary>
            /// An alert or notification sound effect.
            /// </summary>
            Alert,
            /// <summary>
            /// A short tick or click sound effect.
            /// </summary>
            Tick,
            /// <summary>
            /// A chime or bell sound effect.
            /// </summary>
            Chime,
            /// <summary>
            /// A buzzing or vibrating sound effect.
            /// </summary>
            Buzz,
            /// <summary>
            /// A fanfare or celebratory sound effect.
            /// </summary>
            Fanfare,
            /// <summary>
            /// A short click sound effect.
            /// </summary>
            Click,
            /// <summary>
            /// A popping sound effect.
            /// </summary>
            Pop,
            /// <summary>
            /// A power-up sound effect.
            /// </summary>
            PowerUp,
            /// <summary>
            /// A power-down sound effect.
            /// </summary>
            PowerDown,
            /// <summary>
            /// A notification sound effect.
            /// </summary>
            Notification
        }

        private readonly int defaultDuration;
        private readonly int defaultPause;
        private readonly IToneGenerator toneGenerator;

        public SoundEffects(IToneGenerator toneGenerator, int defaultDuration = 100, int defaultPause = 50)
        {
            this.toneGenerator = toneGenerator;
            this.defaultDuration = defaultDuration;
            this.defaultPause = defaultPause;
        }

        public async Task PlayEffect(Effect effect)
        {
            switch (effect)
            {
                case Effect.Beep:
                    await PlayBeep();
                    break;
                case Effect.Success:
                    await PlaySuccess();
                    break;
                case Effect.Failure:
                    await PlayFailure();
                    break;
                case Effect.Warning:
                    await PlayWarning();
                    break;
                case Effect.Alarm:
                    await PlayAlarm();
                    break;
                case Effect.Tick:
                    await PlayTick();
                    break;
                case Effect.Chime:
                    await PlayChime();
                    break;
                case Effect.Buzz:
                    await PlayBuzz();
                    break;
                case Effect.Fanfare:
                    await PlayFanfare();
                    break;
                case Effect.Alert:
                    await PlayAlert();
                    break;
                case Effect.Click:
                    await PlayClick();
                    break;
                case Effect.Pop:
                    await PlayPop();
                    break;
                case Effect.PowerUp:
                    await PlayPowerUp();
                    break;
                case Effect.PowerDown:
                    await PlayPowerDown();
                    break;
                case Effect.Notification:
                    await PlayNotification();
                    break;
                default:
                    throw new ArgumentException($"Unknown effect: {effect}");
            }
        }

        private async Task PlayBeep()
        {
            await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration));
        }

        private async Task PlaySuccess()
        {
            await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(1500), TimeSpan.FromMilliseconds(defaultDuration));
        }

        private async Task PlayFailure()
        {
            await toneGenerator.PlayTone(new Frequency(1500), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration));
        }

        private async Task PlayWarning()
        {
            await toneGenerator.PlayTone(new Frequency(500), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(500), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(500), TimeSpan.FromMilliseconds(defaultDuration));
        }

        private async Task PlayAlarm()
        {
            for (int i = 0; i < 5; i++)
            {
                await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration));
                await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            }
        }

        private async Task PlayTick()
        {
            await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        private async Task PlayChime()
        {
            await toneGenerator.PlayTone(new Frequency(262), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(330), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(392), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(523), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a buzzing or vibrating sound effect
        /// </summary>
        private async Task PlayBuzz()
        {
            await toneGenerator.PlayTone(new Frequency(500), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(500), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a fanfare or celebratory sound effect
        /// </summary>
        private async Task PlayFanfare()
        {
            await toneGenerator.PlayTone(new Frequency(784), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(659), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(523), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(784), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(659), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(523), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays an alert or notification sound effect.
        /// </summary>
        private async Task PlayAlert()
        {
            await toneGenerator.PlayTone(new Frequency(1047), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(1175), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(1319), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(1397), TimeSpan.FromMilliseconds(defaultDuration / 8));
        }

        /// <summary>
        /// Plays a short click sound effect.
        /// </summary>
        private async Task PlayClick()
        {
            await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration / 8));
        }

        /// <summary>
        /// Plays a popping sound effect.
        /// </summary>
        private async Task PlayPop()
        {
            await toneGenerator.PlayTone(new Frequency(500), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a power-up sound effect.
        /// </summary>
        private async Task PlayPowerUp()
        {
            await toneGenerator.PlayTone(new Frequency(200), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a power-down sound effect
        /// </summary>
        private async Task PlayPowerDown()
        {
            await toneGenerator.PlayTone(new Frequency(1000), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(200), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a notification sound effect
        /// </summary>
        private async Task PlayNotification()
        {
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await toneGenerator.PlayTone(new Frequency(784), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await toneGenerator.PlayTone(new Frequency(698), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await toneGenerator.PlayTone(new Frequency(784), TimeSpan.FromMilliseconds(defaultDuration / 8));
            await toneGenerator.PlayTone(new Frequency(698), TimeSpan.FromMilliseconds(defaultDuration / 8));
        }
    }
}