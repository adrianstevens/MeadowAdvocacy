using Meadow.Foundation.Audio;
using Skeeball.Songs;

namespace Skeeball.Controllers;

internal class AudioController
{
    readonly MicroAudio audio;

    readonly BunnyBallTheme themeSong;

    public AudioController(PiezoSpeaker speaker)
    {
        audio = new MicroAudio(speaker);

        themeSong = new BunnyBallTheme();
    }

    public void PlayThemeSong()
    {
        audio.PlaySong(themeSong);
    }
}
