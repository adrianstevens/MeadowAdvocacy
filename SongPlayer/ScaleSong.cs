using Meadow.Peripherals.Speakers;
using static SongPlayer.Note;

namespace SongPlayer
{
    internal class ScaleSong : SongPlayer
    {
        public ScaleSong(IToneGenerator speaker) : base(speaker)
        {
            AddNotes();
        }

        void AddNotes()
        {
            for (int i = 0; i < 12; i++)
            {
                AddNote(new Note((NotePitch)(i), 3, Note.NoteDuration.Quarter));
            }
        }
    }
}