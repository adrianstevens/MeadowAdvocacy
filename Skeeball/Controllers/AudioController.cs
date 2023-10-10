﻿using Meadow.Foundation.Audio;
using Meadow.Peripherals.Speakers;
using Skeeball.Songs;

namespace Skeeball.Controllers;

internal class AudioController
{
    readonly MicroAudio audio;

    readonly BunnyBallTheme themeSong;

    public bool PlaySound { get; set; } = false;

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
        audio.PlayGameSound(GameSoundEffect.Blip);
    }
}
