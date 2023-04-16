using Meadow.Units;
using static SongPlayer.Note;

namespace SongPlayer
{
    public class NotesToFrequency
    {
        /// <summary>
        /// Converts a musical note and octave to a frequency in Hz
        /// </summary>
        /// <param name="note">The musical note</param>
        /// <param name="octave">The octave of the note (4 is middle C)</param>
        /// <returns>The frequency of the note in Hz</returns>
        public static Frequency ConvertNoteToFrequency(NotePitch note, int octave)
        {
            if (note == NotePitch.Rest) return new Frequency(0);

            double[] frequencies = new double[]
            {
                261.63, // C4
                277.18, // C#4/Db4
                293.66, // D4
                311.13, // D#4/Eb4
                329.63, // E4
                349.23, // F4
                369.99, // F#4/Gb4
                392.00, // G4
                415.30, // G#4/Ab4
                440.00, // A4
                466.16, // A#4/Bb4
                493.88  // B4
            };

            int index = (int)note;
            index += (octave - 4) * 12;

            return new Frequency(frequencies[index], Frequency.UnitType.Hertz);
        }      
    }
}