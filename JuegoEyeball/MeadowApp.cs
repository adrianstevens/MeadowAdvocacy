using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WildernessLabs.Hardware.Juego;

namespace Froggit
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IJuegoHardware juego;
        MicroGraphics graphics;

        readonly MicroAudio moveAudio;
        readonly MicroAudio effectsAudio;

        IPixelBuffer eyeballBuffer;

        int xOffset = 0;
        int yOffset = 0;

        readonly int MovementStep = 4;
        readonly int ReturnStep = 12;

        readonly int MaxEyeMovement = 50;

        public override async Task Initialize()
        {
            Console.WriteLine("Initialize...");

            juego = Juego.Create();
            juego.StartButton.Clicked += StartButton_Clicked;

            juego.Left_UpButton.Clicked += Left_UpButton_Clicked;
            juego.Left_DownButton.Clicked += Left_DownButton_Clicked;
            juego.Left_LeftButton.Clicked += Left_LeftButton_Clicked;
            juego.Left_RightButton.Clicked += Left_RightButton_Clicked;

            graphics = new MicroGraphics(juego.Display)
            {
                CurrentFont = new Font12x16(),
            };

            InitializeEyeballBuffer();

            Console.WriteLine("Initialize complete");
        }

        void InitializeEyeballBuffer()
        {
            eyeballBuffer = new BufferRgb565(240, 240);

            var eyeballGraphics = new MicroGraphics(eyeballBuffer as PixelBufferBase, false);

            eyeballGraphics.Clear();

            eyeballGraphics.DrawCircle(eyeballGraphics.Width / 2, eyeballGraphics.Height / 2, 120, outline, true, true);
            eyeballGraphics.DrawCircle(eyeballGraphics.Width / 2, eyeballGraphics.Height / 2, 112, red, true, true);
            eyeballGraphics.DrawCircle(eyeballGraphics.Width / 2, eyeballGraphics.Height / 2, 104, pink, true, true);
            eyeballGraphics.DrawCircle(eyeballGraphics.Width / 2, eyeballGraphics.Height / 2, 96, light, true, true);
        }

        private void Left_RightButton_Clicked(object sender, EventArgs e)
        {
            if (xOffset < MaxEyeMovement)
            {
                xOffset += MovementStep;
            }
            DrawEyeball();
        }

        private void Left_LeftButton_Clicked(object sender, EventArgs e)
        {
            LookLeft();
            Thread.Sleep(1000);
            CenterEye();
        }

        private void Left_DownButton_Clicked(object sender, EventArgs e)
        {
            if (yOffset < MaxEyeMovement)
            {
                yOffset += MovementStep;
            }
            DrawEyeball();
        }

        private void Left_UpButton_Clicked(object sender, EventArgs e)
        {
            if (yOffset > -MaxEyeMovement)
            {
                yOffset -= MovementStep;
            }
            DrawEyeball();
        }

        public override async Task Run()
        {
            Console.WriteLine("Run...");

            while (true)
            {
                LookLeft();
                Thread.Sleep(1000);
                CenterEye();
                Thread.Sleep(1000);
                Blink();

                LookRight();
                Thread.Sleep(1000);
                CenterEye();
                Thread.Sleep(1000);
                Blink();

                LookUp();
                Thread.Sleep(1000);
                CenterEye();
                Thread.Sleep(1000);
                Blink();

                LookDown();
                Thread.Sleep(1000);
                CenterEye();
                Thread.Sleep(1000);
                Blink();
            }


        }

        void ResetEye()
        {
            xOffset = 0;
            yOffset = 0;
            DrawEyeball();
        }

        void Blink()
        {
            DrawEyeball();
            graphics.DrawCircle(graphics.Width / 2, graphics.Height / 2, 120, outline, true, true);
            graphics.Show();
            Thread.Sleep(50);
            DrawEyeball();
            Thread.Sleep(100);
            graphics.DrawCircle(graphics.Width / 2, graphics.Height / 2, 120, outline, true, true);
            graphics.Show();
            Thread.Sleep(50);
            DrawEyeball();

        }

        void LookLeft()
        {
            ResetEye();

            for (int i = 0; i < MaxEyeMovement; i += MovementStep)
            {
                xOffset = -i;
                DrawEyeball();
            }
        }

        void LookRight()
        {
            ResetEye();

            for (int i = 0; i < MaxEyeMovement; i += MovementStep)
            {
                xOffset = i;
                DrawEyeball();
            }
        }

        void LookUp()
        {
            ResetEye();

            for (int i = 0; i < MaxEyeMovement; i += MovementStep)
            {
                yOffset = -i;
                DrawEyeball();
            }
        }

        void LookDown()
        {
            ResetEye();

            for (int i = 0; i < MaxEyeMovement; i += MovementStep)
            {
                yOffset = i;
                DrawEyeball();
            }
        }

        void CenterEye()
        {
            while (xOffset != 0 || yOffset != 0)
            {
                if (xOffset > 0)
                {
                    xOffset -= ReturnStep;
                    if (xOffset < 0)
                        xOffset = 0;
                }
                else if (xOffset < 0)
                {
                    xOffset += ReturnStep;
                    if (xOffset > 0)
                        xOffset = 0;
                }

                if (yOffset > 0)
                {
                    yOffset -= ReturnStep;
                    if (yOffset < 0)
                        yOffset = 0;
                }
                else if (yOffset < 0)
                {
                    yOffset += ReturnStep;
                    if (yOffset > 0)
                        yOffset = 0;
                }

                DrawEyeball();
            }
        }

        private void StartButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartButton_Clicked");

        }

        Color outline = Color.FromHex("241E25");

        Color red = Color.FromHex("F27274");
        Color pink = Color.FromHex("F9C0C5");
        Color light = Color.FromHex("FFEADD");

        Color darkGreen = Color.FromHex("367D17");
        Color lightGreen = Color.FromHex("55B835");

        readonly Stopwatch stopwatch = new Stopwatch();

        void DrawEyeball()
        {
            //add timer profiling for this method and write to console 
            stopwatch.Restart();

            graphics.DrawBuffer(graphics.Width / 2 - eyeballBuffer.Width / 2,
                graphics.Height / 2 - eyeballBuffer.Height / 2, eyeballBuffer);

            graphics.DrawCircle(graphics.Width / 2 + xOffset, graphics.Height / 2 + yOffset, 40, darkGreen, true, true);
            graphics.DrawCircle(graphics.Width / 2 + xOffset, graphics.Height / 2 + yOffset, 32, lightGreen, true, true);

            graphics.DrawCircle(graphics.Width / 2 + xOffset * 5 / 4, graphics.Height / 2 + yOffset * 5 / 4, 16, Color.Black, true, true);

            graphics.Show();

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");
        }
    }
}