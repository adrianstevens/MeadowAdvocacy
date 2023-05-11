using Froggit.Services;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Froggit
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV1>
    {
        FrogItGame game;
        MicroGraphics graphics;
        MicroAudio audio;

        IWiFiNetworkAdapter wifi;

        private const string WIFI_NAME = "TELUSDC1E"; //"BunnyMesh";
        private const string WIFI_PASSWORD = "tnrXFa6MVqAU"; //"zxpvi29wt8";

        GameState gameState = GameState.Ready;

        enum GameState
        {
            Ready,
            Playing,
            GameOver
        }

        public override async Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var spiBus = Device.CreateSpiBus(new Meadow.Units.Frequency(48000, Meadow.Units.Frequency.UnitType.Kilohertz));

            var display = new Ili9341(spiBus, Device.Pins.D03, Device.Pins.D02, Device.Pins.D01, 240, 320);

            display.SetRotation(RotationType._270Degrees);
            display.SpiBusSpeed = new Meadow.Units.Frequency(48000, Meadow.Units.Frequency.UnitType.Kilohertz);

            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font12x16(),
            };

            game = new FrogItGame();

            game.Init(graphics, audio);

            Console.WriteLine("Initialize complete");
        }

        public async override Task Run()
        {
            Console.WriteLine("Run...");

            //await HighScoreService.PostTime("Juego0", 1000, 20);
            //await HighScoreService.PostTime("Juego0", 500, 15);

            gameState = GameState.Playing;

            await PlayGame();

            //  return Task.CompletedTask;
        }

        void UpdateGame()
        {
            game.Update();
        }

        void DrawplashScreen()
        {
            graphics.Clear();
            graphics.DrawText(160, 70, "Froggit", FrogItGame.FrogColor, ScaleFactor.X3, HorizontalAlignment.Center);
            graphics.DrawText(160, 140, "Press Start", FrogItGame.WaterColor, ScaleFactor.X1, HorizontalAlignment.Center);
            graphics.Show();
        }

        void DrawEndScreen()
        {
            graphics.Clear();

            if (game.Winner)
            {
                graphics.DrawText(160, 80, "You Win!", FrogItGame.FrogColor, ScaleFactor.X3, HorizontalAlignment.Center);
                graphics.DrawText(160, 140, $"Your time: {game.GameTime:F1}s", FrogItGame.WaterColor, ScaleFactor.X1, HorizontalAlignment.Center);
                graphics.DrawText(160, 160, $"Your died: {game.Deaths} time(s)", FrogItGame.WaterColor, ScaleFactor.X1, HorizontalAlignment.Center);

                _ = HighScoreService.PostTime("Juego1", (int)game.GameTime, game.Deaths);
            }
            else
            {
                graphics.DrawText(160, 80, "Game Over", FrogItGame.FrogColor, ScaleFactor.X3, HorizontalAlignment.Center);
            }

            graphics.Show();
        }

        Task PlayGame()
        {
            var t = new Task(() =>
            {
                game.Reset();

                Console.WriteLine("Start game loop");

                while (game.IsPlaying)
                {
                    UpdateGame();
                    Thread.Sleep(0);
                }
                gameState = GameState.GameOver;
                DrawEndScreen();
            }, TaskCreationOptions.LongRunning);

            t.Start();

            return t;
        }
    }
}