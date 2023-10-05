using Meadow.Foundation.Audio;

namespace Skeeball.Songs;

internal class BunnyBallTheme : Song
{
    public BunnyBallTheme()
    {
        AddNotes();
    }

    void AddNotes()
    {
        AddNote(new Note(Pitch.C, 2, NoteDuration.Half));

        //bar 1
        AddNote(new Note(Pitch.C, 2, NoteDuration.Eighth));
        AddNote(new Note(Pitch.C, 2, NoteDuration.Sixteenth));
        AddNote(new Note(Pitch.C, 2, NoteDuration.Sixteenth));
        AddNote(new Note(Pitch.C, 2, NoteDuration.Eighth));
        AddNote(new Note(Pitch.C, 2, NoteDuration.Eighth));

        //bar 2
        AddNote(new Note(Pitch.E, 2, NoteDuration.Eighth));
        AddNote(new Note(Pitch.E, 2, NoteDuration.Sixteenth));
        AddNote(new Note(Pitch.E, 2, NoteDuration.Sixteenth));
        AddNote(new Note(Pitch.E, 2, NoteDuration.Eighth));
        AddNote(new Note(Pitch.E, 2, NoteDuration.Eighth));

        //bar 3
        AddNote(new Note(Pitch.G, 2, NoteDuration.Eighth));
        AddNote(new Note(Pitch.G, 2, NoteDuration.Sixteenth));
        AddNote(new Note(Pitch.G, 2, NoteDuration.Sixteenth));
        AddNote(new Note(Pitch.G, 2, NoteDuration.Eighth));
        AddNote(new Note(Pitch.G, 2, NoteDuration.Eighth));

        //bar4
        AddNote(new Note(Pitch.C, 3, NoteDuration.Eighth));
        AddNote(new Note(Pitch.C, 3, NoteDuration.Sixteenth));
        AddNote(new Note(Pitch.C, 3, NoteDuration.Sixteenth));
    }
}
