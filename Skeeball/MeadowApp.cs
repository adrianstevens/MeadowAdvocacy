using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;
using WildernessLabs.Hardware.Juego;

namespace Skeeball
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        SkeeballGame game;

        DisplayController displayController;

        MicroGraphics graphics;

        IJuegoHardware juego;

        IWiFiNetworkAdapter wifi;

        private const string WIFI_NAME = "BunnyMesh";
        private const string WIFI_PASSWORD = "zxpvi29wt8";

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            game = new SkeeballGame();

            juego = Juego.Create();

            graphics = new MicroGraphics(juego.Display)
            {
                CurrentFont = new Font4x8(),
                Rotation = RotationType._270Degrees
            };

            displayController = new DisplayController(graphics);

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            wifi.NetworkConnected += Wifi_NetworkConnected;

            /* ... will use it later but commented out for perf
            try
            {
                // connect to the wifi network.
                Resolver.Log.Info($"Connecting to WiFi Network {WIFI_NAME}");

                _ = wifi.Connect(WIFI_NAME, WIFI_PASSWORD, TimeSpan.FromSeconds(45));
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect: {ex.Message}");
            }
            */

            juego.StartButton.Clicked += StartButton_Clicked;
            juego.StartButton.LongClicked += StartButton_LongClicked;
            juego.SelectButton.Clicked += SelectButton_Clicked;

            juego.Right_UpButton.Clicked += Right_UpButton_Clicked;
            juego.Right_LeftButton.Clicked += Right_LeftButton_Clicked;
            juego.Right_DownButton.Clicked += Right_DownButton_Clicked;
            juego.Right_RightButton.Clicked += Right_RightButton_Clicked;

            Console.WriteLine("Init complete");
            return base.Initialize();
        }

        private void StartButton_LongClicked(object sender, EventArgs e)
        {
            game.Reset();
        }

        private void Right_RightButton_Clicked(object sender, EventArgs e)
        {
            game.ThrowBall(SkeeballGame.PointValue.Twenty);
            ShowScore(SkeeballGame.PointValue.Twenty);
        }

        private void Right_DownButton_Clicked(object sender, EventArgs e)
        {
            game.ThrowBall(SkeeballGame.PointValue.Ten);
            ShowScore(SkeeballGame.PointValue.Ten);
        }

        private void Right_LeftButton_Clicked(object sender, EventArgs e)
        {
            game.ThrowBall(SkeeballGame.PointValue.Forty);
            ShowScore(SkeeballGame.PointValue.Forty);
        }

        private void Right_UpButton_Clicked(object sender, EventArgs e)
        {
            game.ThrowBall(SkeeballGame.PointValue.Fifty);
            ShowScore(SkeeballGame.PointValue.Fifty);
        }

        private void SelectButton_Clicked(object sender, EventArgs e)
        {

        }

        private void StartButton_Clicked(object sender, EventArgs e)
        {
            if (game.StartGame())
            {
                ShowBallsRemaining();
            }
        }

        public override Task Run()
        {
            displayController.DrawTitle();

            return base.Run();
        }

        void ShowBallsRemaining()
        {
            displayController.DrawText($"BALLS: {game.CurrentPlayer.BallsRemaining}", Color.White);
        }

        void ShowScore(SkeeballGame.PointValue value)
        {
            displayController.FlashText($"{(int)value}", Color.Blue, Color.Violet);
            displayController.DrawText($"{game.CurrentPlayer.Score}", Color.White);
            Thread.Sleep(1000);
            ShowBallsRemaining();
        }

        void UpdateScore(SkeeballGame.PointValue points)
        {

        }


        void DisplayTest()
        {
            while (true)
            {
                displayController.DrawTitle();
                Thread.Sleep(1000);

                displayController.FlashText("10", Color.Red, Color.Purple);
                displayController.FlashText("20", Color.Orange, Color.Red);
                displayController.FlashText("30", Color.Yellow, Color.Orange);
                displayController.FlashText("40", Color.YellowGreen, Color.Yellow);
                displayController.FlashText("50", Color.Green, Color.YellowGreen);
                Thread.Sleep(1000);

                displayController.ScrollTextOn("FREEBALL", Color.Yellow);
                displayController.FlashText("FREEBALL", Color.YellowGreen, Color.Yellow);
                Thread.Sleep(1000);

                displayController.DrawText("SCORE", Color.Cyan);
                Thread.Sleep(250);
                displayController.ScrollTextOn("400", Color.Cyan);
                Thread.Sleep(1000);
                displayController.DrawText("BALLS: 9", Color.White);
                Thread.Sleep(1000);
            }
        }

        private async void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {

        }


    }
}