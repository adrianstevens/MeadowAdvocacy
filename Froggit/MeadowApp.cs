using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;
using WildernessLabs.Hardware.Juego;

namespace Froggit
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;
        FrogItGame game;
        MicroGraphics graphics;

        bool playGame = true;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();
            juego.Left_DownButton.Clicked += Left_DownButton_Clicked;
            juego.Left_UpButton.Clicked += Left_UpButton_Clicked;
            juego.Left_LeftButton.Clicked += Left_LeftButton_Clicked;
            juego.Left_RightButton.Clicked += Left_RightButton_Clicked;

            graphics = new MicroGraphics(juego.Display);
            graphics.CurrentFont = new Font8x12();
            graphics.Rotation = RotationType._270Degrees;

            game = new FrogItGame();
            game.Init(graphics);


            return base.Initialize();
        }

        private void Left_RightButton_Clicked(object sender, EventArgs e)
        {
            game.Right();
        }

        private void Left_LeftButton_Clicked(object sender, EventArgs e)
        {
            game.Left();
        }

        private void Left_UpButton_Clicked(object sender, EventArgs e)
        {
            game.Up();
        }

        private void Left_DownButton_Clicked(object sender, EventArgs e)
        {
            game.Down();
        }

        public async override Task Run()
        {
            Console.WriteLine("Run...");

            await Task.Run(() =>
            {   //full speed today
                while (playGame == true)
                {
                    game.Update(graphics);

                    Thread.Sleep(0);
                }
            });
        }


    }
}