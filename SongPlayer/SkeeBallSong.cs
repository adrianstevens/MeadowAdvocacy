using Meadow.Foundation.Audio;
using static SongPlayer.Note;

namespace SongPlayer
{
    internal class SkeeBallSong : SongPlayer
    {
        public SkeeBallSong(PiezoSpeaker speaker) : base(speaker)
        {
            AddNotes();
        }

        void AddNotes()
        {
            AddNote(new Note(NotePitch.C, 2, NoteDuration.Half));

            //bar 1
            AddNote(new Note(NotePitch.C, 3, NoteDuration.Eighth));
            AddNote(new Note(NotePitch.C, 3, NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.C, 3, NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.C, 3, NoteDuration.Eighth));
            AddNote(new Note(NotePitch.C, 3, NoteDuration.Eighth));

            //bar 2
            AddNote(new Note(NotePitch.E, 3, NoteDuration.Eighth));
            AddNote(new Note(NotePitch.E, 3, NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.E, 3, NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.E, 3, NoteDuration.Eighth));
            AddNote(new Note(NotePitch.E, 3, NoteDuration.Eighth));

            //bar 3
            AddNote(new Note(NotePitch.G, 3, NoteDuration.Eighth));
            AddNote(new Note(NotePitch.G, 3, NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.G, 3, NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.G, 3, NoteDuration.Eighth));
            AddNote(new Note(NotePitch.G, 3, NoteDuration.Eighth));

            //bar4
            AddNote(new Note(NotePitch.C, 4, NoteDuration.Quarter));
            AddNote(new Note(NotePitch.C, 4, NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.C, 4, NoteDuration.Sixteenth));
        }
    }
}
