﻿using Meadow;
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



            Console.WriteLine("Play scale song");
            var scale = new ScaleSong(projLab.Speaker);
            await scale.Play(120);

            await Task.Delay(3000);

            Console.WriteLine("Play skeeball song");
            var skeelballSong = new SkeeBallSong(projLab.Speaker);
            await skeelballSong.Play(120);

            await Task.Delay(3000);

            Console.WriteLine("Play happy birthday");
            SongPlayer happyBirthday = new SongPlayer(projLab.Speaker);
            happyBirthday.AddNote(new Note(NotePitch.C, 4, NoteDuration.Quarter));
            happyBirthday.AddNote(new Note(NotePitch.C, 4, NoteDuration.Quarter));
            happyBirthday.AddNote(new Note(NotePitch.D, 4, NoteDuration.Half));
            happyBirthday.AddNote(new Note(NotePitch.C, 4, NoteDuration.Half));
            happyBirthday.AddNote(new Note(NotePitch.F, 4, NoteDuration.Half));
            happyBirthday.AddNote(new Note(NotePitch.E, 4, NoteDuration.Whole));
         
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
    }
}