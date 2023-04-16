using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SongPlayer.Note;

namespace SongPlayer
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        IProjectLabHardware projLab;

        MicroGraphics graphics;

        public async override Task Run()
        {
            Console.WriteLine("Run...");

            SongPlayer happyBirthday = new SongPlayer(projLab.Speaker);
            happyBirthday.AddNote(new Note(NotePitch.C, 4, NoteDuration.Quarter)); // C4 quarter note
            happyBirthday.AddNote(new Note(NotePitch.C, 4, NoteDuration.Quarter)); // C4 quarter note
            happyBirthday.AddNote(new Note(NotePitch.D, 4, NoteDuration.Half)); // D4 half note
            happyBirthday.AddNote(new Note(NotePitch.C, 4, NoteDuration.Half)); // C4 half note
            happyBirthday.AddNote(new Note(NotePitch.F, 4, NoteDuration.Half)); // F4 half note
            happyBirthday.AddNote(new Note(NotePitch.E, 4, NoteDuration.Whole)); // E4 whole note
         
            await happyBirthday.Play(120); // play at 120 beats per minute

        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            Console.WriteLine("Init complete");
            return base.Initialize();
        }
    }
}