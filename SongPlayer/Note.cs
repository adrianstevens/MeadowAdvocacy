namespace SongPlayer
{
    public class Note
    {
        /// <summary>
        /// Represents a musical note
        /// </summary>
        public enum NotePitch : int
        {
            C = 0,
            CSharp = 1,
            D = 2,
            DSharp = 3,
            E = 4,
            F = 5,
            FSharp = 6,
            G = 7,
            GSharp = 8,
            A = 9,
            ASharp = 10,
            B = 11,
            Rest
        }

        public enum NoteDuration
        {
            Whole = 4000,
            Half = 2000,
            Quarter = 1000,
            Eighth = 500,
            Sixteenth = 250,
            ThirtySecond = 125
        }

        public NotePitch Pitch { get; }
        public int Octave { get; }
        public NoteDuration Duration { get; }

        public Note(NotePitch pitch, int octave, NoteDuration duration)
        {
            Pitch = pitch;
            Octave = octave;
            Duration = duration;
        }
    }
}