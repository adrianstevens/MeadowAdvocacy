using Meadow.Foundation.Audio;
using System;
using System.Collections.Generic;
using static SongPlayer.Note;

namespace SongPlayer
{
    internal class ScaleSong : SongPlayer
    {
        public ScaleSong(PiezoSpeaker speaker) : base(speaker)
        {
            AddNotes();
        }

        void AddNotes()
        {
            for(int i = 0; i < 12; i++)
            {
                AddNote(new Note((NotePitch)(i), 3, Note.NoteDuration.Quarter));
            }
        }
    }
}
