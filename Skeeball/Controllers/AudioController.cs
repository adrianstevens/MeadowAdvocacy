using Meadow.Foundation.Audio;
using Meadow.Peripherals.Speakers;
using Skeeball.Songs;

namespace Skeeball.Controllers;

internal class AudioController
{
    readonly MicroAudio audio;

    readonly BunnyBallTheme themeSong;

    public bool PlaySound { get; set; } = true;

    internal AudioController(IToneGenerator speaker)
    {
        audio = new MicroAudio(speaker);

        themeSong = new BunnyBallTheme();
    }

    internal void PlayThemeSong()
    {
        if (!PlaySound) return;
        audio.PlaySong(themeSong);
    }

    internal void PlayScoreSound(SkeeballGame.PointValue value)
    {
        if (!PlaySound) return;

        switch (value)
        {
            case SkeeballGame.PointValue.Ten:
                _ = audio.PlayGameSound(GameSoundEffect.Blip);
                break;
            case SkeeballGame.PointValue.Twenty:
                _ = audio.PlayGameSound(GameSoundEffect.Coin);
                break;
            case SkeeballGame.PointValue.Thirty:
                _ = audio.PlayGameSound(GameSoundEffect.Teleport);
                break;
            case SkeeballGame.PointValue.Forty:
                _ = audio.PlayGameSound(GameSoundEffect.SecretFound);
                break;
            case SkeeballGame.PointValue.Fifty:
                _ = audio.PlayGameSound(GameSoundEffect.PowerUp);
                break;
        }
    }
}
