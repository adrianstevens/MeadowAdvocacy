﻿using Meadow.Foundation.Audio;
using Meadow.Peripherals.Speakers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SongPlayer
{
    public class SongPlayer
    {
        private readonly List<Note> notes = new List<Note>();

        IToneGenerator speaker;

        public SongPlayer(IToneGenerator toneGenerator)
        {
            this.speaker = toneGenerator;
        }

        public void AddNote(Note note)
        {
            notes.Add(note);
        }

        public async Task Play(int tempo = 120)
        {
            foreach (var note in notes)
            {
                
                int duration = (int)(60.0 / tempo * (int)note.Duration);
                Console.WriteLine($"Duration in ms: {duration}");


                if(note.Pitch == Note.NotePitch.Rest)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(duration));
                }
                else
                {
                    var frequency = NotesToFrequency.ConvertToFrequency(note);
                    await speaker.PlayTone(frequency, TimeSpan.FromMilliseconds(duration));
                }
            }
        }
    }
}