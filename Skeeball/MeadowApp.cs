using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using SimpleJpegDecoder;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Skeeball
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        SkeeballGame game;

        DisplayController displayController;

        MicroGraphics graphicsTop;
        MicroGraphics graphicsBottom;

        IProjectLabHardware projLab;
        Apa102 topDisplay;

        IPixelBuffer bunny1, bunny2;

        readonly Random random = new Random();

        IWiFiNetworkAdapter wifi;

        private const string WIFI_NAME = "BunnyMesh";
        private const string WIFI_PASSWORD = "zxpvi29wt8";

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            Console.WriteLine("1");

            projLab = ProjectLab.Create();

            projLab.DownButton.Clicked += StartButton_Clicked;
            projLab.UpButton.Clicked += SelectButton_Clicked;

            projLab.LeftButton.Clicked += RandomThrow;

            projLab.RightButton.PressStarted += RightButton_PressStarted;

            graphicsBottom = new MicroGraphics(projLab.Display)
            {
                CurrentFont = new Font8x12(),
                Rotation = RotationType._180Degrees
            };

            topDisplay = new Apa102(projLab.MikroBus1.SpiBus, 32, 8);

            topDisplay.Clear();
            topDisplay.DrawPixel(0, 0, Color.Blue);
            topDisplay.DrawPixel(1, 1, Color.Green);
            topDisplay.Show();

            graphicsTop = new MicroGraphics(topDisplay)
            {
                CurrentFont = new Font4x8(),
            };

            graphicsTop.DrawLine(0, 0, 5, 5, Color.Green);
            graphicsTop.Show();

            displayController = new DisplayController(graphicsTop, graphicsBottom);

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

            game = new SkeeballGame();

            bunny1 = LoadImage("bunny1.jpg");
            bunny2 = LoadImage("bunny2.jpg");

            Console.WriteLine("Init complete");
            return Task.CompletedTask;
        }

        void DrawSplash()
        {
            graphicsBottom.Clear();

            graphicsBottom.DrawBuffer(graphicsBottom.Width / 2 - bunny1.Width / 2, graphicsBottom.Height / 2 - bunny1.Height / 2, bunny1);

            //draw the text BunnyBall below the image of the bunny
            graphicsBottom.DrawText(120, 200, "BunnyBall", Color.White, ScaleFactor.X2, HorizontalAlignment.Center);

            graphicsBottom.Show();
        }

        private void RightButton_PressStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Started");
        }

        private void StartButton_LongClicked(object sender, EventArgs e)
        {
            game.Reset();
        }

        private void RandomThrow(object sender, EventArgs e)
        {
            Console.WriteLine("RandomThrow");

            var randomValue = random.Next(1, 6) * 10;

            ThrowBall((SkeeballGame.PointValue)randomValue);
        }

        void ThrowBall(SkeeballGame.PointValue pointValue)
        {
            if (!game.ThrowBall(pointValue))
            {
                return;
            }

            ShowScore(pointValue);
            ShowBallsRemaining();

            if (game.CurrentState == SkeeballGame.GameState.GameOver)
            {
                displayController.FlashText($"GAMEOVER", Color.Red, Color.Yellow);
                displayController.DrawText("GAMEOVER", Color.Red);
                Thread.Sleep(2000);
                displayController.ScrollTextOn("YOUR SCORE:", Color.Red);
                Thread.Sleep(150);
                displayController.FlashText($"{game.CurrentPlayer.Score}", Color.LawnGreen, Color.Cyan);

                DrawSplash();
            }
        }

        private void SelectButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("SelectButton_Clicked");
        }

        private void StartButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartButton_Clicked");

            if (game.StartGame())
            {
                displayController.DrawText("READY", Color.LawnGreen);
                ShowBallsRemaining();
            }
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run ...");

            _ = projLab.RgbLed.StartBlink(Color.LawnGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);

            displayController.DrawTitle();
            game.Reset();

            DrawSplash();

            return Task.CompletedTask;
        }

        void ShowBallsRemaining()
        {
            graphicsBottom.Clear();
            graphicsBottom.DrawText(120, 160, $"{game.CurrentPlayer.BallsRemaining}", Color.LawnGreen, (ScaleFactor)16, HorizontalAlignment.Center, VerticalAlignment.Center);

            int yStart = (4 - game.CurrentPlayer.BallsRemaining / 2) * bunny2.Height;

            for (int i = 0; i < game.CurrentPlayer.BallsRemaining; i++)
            {
                graphicsBottom.DrawBuffer(205, yStart + i * bunny2.Height, bunny2);
            }

            graphicsBottom.Show();
        }

        void ShowScore(SkeeballGame.PointValue value)
        {
            displayController.FlashText($"{(int)value}", Color.Blue, Color.Violet);
            displayController.DrawText($"{game.CurrentPlayer.Score}", Color.White);
        }

        private async void Wifi_NetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
        }

        IPixelBuffer LoadImage(string name)
        {
            var jpgData = LoadResource(name);

            Console.WriteLine($"Loaded {jpgData.Length} bytes, decoding jpeg ...");

            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(jpgData);

            Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes");
            Console.WriteLine($"Width {decoder.Width}");
            Console.WriteLine($"Height {decoder.Height}");

            return new BufferRgb888(decoder.Width, decoder.Height, jpg).ConvertPixelBuffer<BufferRgb565>();
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Skeeball.{filename}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}