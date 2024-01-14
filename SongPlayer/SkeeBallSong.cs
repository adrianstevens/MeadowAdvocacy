using Meadow.Peripherals.Speakers;
using static SongPlayer.Note;

namespace SongPlayer
{
    internal class SkeeBallSong : SongPlayer
    {
        public SkeeBallSong(IToneGenerator speaker) : base(speaker)
        {
            AddNotes();
        }

        void AddNotes()
        {
            AddNote(new Note(NotePitch.C, 2, Note.NoteDuration.Half));

            //bar 1
            AddNote(new Note(NotePitch.C, 3, Note.NoteDuration.Eighth));
            AddNote(new Note(NotePitch.C, 3, Note.NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.C, 3, Note.NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.C, 3, Note.NoteDuration.Eighth));
            AddNote(new Note(NotePitch.C, 3, Note.NoteDuration.Eighth));

            //bar 2
            AddNote(new Note(NotePitch.E, 3, Note.NoteDuration.Eighth));
            AddNote(new Note(NotePitch.E, 3, Note.NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.E, 3, Note.NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.E, 3, Note.NoteDuration.Eighth));
            AddNote(new Note(NotePitch.E, 3, Note.NoteDuration.Eighth));

            //bar 3
            AddNote(new Note(NotePitch.G, 3, Note.NoteDuration.Eighth));
            AddNote(new Note(NotePitch.G, 3, Note.NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.G, 3, Note.NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.G, 3, Note.NoteDuration.Eighth));
            AddNote(new Note(NotePitch.G, 3, Note.NoteDuration.Eighth));

            //bar4
            AddNote(new Note(NotePitch.C, 4, Note.NoteDuration.Quarter));
            AddNote(new Note(NotePitch.C, 4, Note.NoteDuration.Sixteenth));
            AddNote(new Note(NotePitch.C, 4, Note.NoteDuration.Sixteenth));
        }
    }
}