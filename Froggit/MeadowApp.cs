using Froggit.Services;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
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
        MicroAudio audio;

        IWiFiNetworkAdapter wifi;

        private const string WIFI_NAME = "";
        private const string WIFI_PASSWORD = "";

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

            juego = Juego.Create();
            juego.StartButton.Clicked += StartButton_Clicked;

            graphics = new MicroGraphics(juego.Display)
            {
                CurrentFont = new Font12x16(),
            };

            audio = new MicroAudio(juego.RightSpeaker);

            game = new FrogItGame();

            game.Init(graphics, audio);

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            try
            {
                Resolver.Log.Info($"Connecting to WiFi Network {WIFI_NAME}");
                await wifi.Connect(WIFI_NAME, WIFI_PASSWORD, TimeSpan.FromSeconds(45));
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect: {ex.Message}");
            }

            Console.WriteLine("Initialize complete");
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            await HighScoreService.PostTime("Juego0", 1000, 20);
            await HighScoreService.PostTime("Juego0", 500, 15);

            DrawplashScreen();
        }

        bool isInitialized = false;
        private void StartButton_Clicked(object sender, EventArgs e)
        {
            if (isInitialized == false)
            {
                isInitialized = true;
                return;
            }

            if (GameState.Ready == gameState)
            {
                gameState = GameState.Playing;
                _ = PlayGame();
            }
            else if (GameState.GameOver == gameState)
            {
                gameState = GameState.Ready;
                DrawplashScreen();
            }
        }

        void UpdateGame()
        {
            if (juego.Left_LeftButton.State == true)
            {
                game.Left();
            }
            else if (juego.Left_RightButton.State == true)
            {
                game.Right();
            }
            else if (juego.Left_UpButton.State == true)
            {
                game.Up();
            }
            else if (juego.Left_DownButton.State == true)
            {
                game.Down();
            }
            else if (juego.SelectButton.State == true)
            {
                game.Quit();
            }

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