using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Speakers;
using System;
using System.Threading.Tasks;
using static SongPlayer.GameSounds;
using static SongPlayer.Note;
using static SongPlayer.SystemSounds;

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

            await GameEffectsTest(projLab.Speaker);

            await SoundEffectsTest(projLab.Speaker);



            Console.WriteLine("Play scale song");
            var scale = new ScaleSong(projLab.Speaker);
            //   await scale.Play(120);

            ////   await Task.Delay(3000);

            Console.WriteLine("Play skeeball song");
            var skeelballSong = new SkeeBallSong(projLab.Speaker);
            await skeelballSong.Play(120);

            await Task.Delay(3000);

            Console.WriteLine("Play happy birthday");
            SongPlayer happyBirthday = new SongPlayer(projLab.Speaker);
            happyBirthday.AddNote(new Note(NotePitch.C, 3, NoteDuration.Quarter));
            happyBirthday.AddNote(new Note(NotePitch.C, 3, NoteDuration.Quarter));
            happyBirthday.AddNote(new Note(NotePitch.D, 3, NoteDuration.Half));
            happyBirthday.AddNote(new Note(NotePitch.C, 3, NoteDuration.Half));
            happyBirthday.AddNote(new Note(NotePitch.F, 3, NoteDuration.Half));
            happyBirthday.AddNote(new Note(NotePitch.E, 3, NoteDuration.Whole));

            await happyBirthday.Play(160);

        }

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            projLab = ProjectLab.Create();

            graphics = new MicroGraphics(projLab.Display);

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        async Task GameEffectsTest(IToneGenerator piezo)
        {
            var player = new GameSounds(piezo);

            foreach (GameSoundEffect effect in Enum.GetValues(typeof(GameSoundEffect)))
            {
                Console.WriteLine($"Playing {effect} game effect...");
                await player.PlayEffect(effect);
                await Task.Delay(1000);
            }

            Console.WriteLine("Sound effects demo complete.");
        }

        async Task SoundEffectsTest(IToneGenerator piezo)
        {
            var player = new SystemSounds(piezo);

            foreach (SystemSoundEffect effect in Enum.GetValues(typeof(SystemSoundEffect)))
            {
                Console.WriteLine($"Playing {effect} sound effect...");
                await player.PlayEffect(effect);
                await Task.Delay(1000);
            }

            Console.WriteLine("Sound effects demo complete.");
        }
    }
}