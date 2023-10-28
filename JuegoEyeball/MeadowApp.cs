using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
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
        MicroGraphics graphics;

        IPixelBuffer eyeballBuffer;

        int xOffset = 0;
        int yOffset = 0;

        int xLast = -1;
        int yLast = -1;

        readonly int MovementStep = 4;
        readonly int ReturnStep = 12;

        readonly int MinEyeMovement = 10;
        readonly int MaxEyeMovement = 50;

        readonly Random random = new Random();

        public enum EyeMovement
        {
            LookLeft,
            LookRight,
            LookUp,
            LookDown,
            Blink,
            Dilate,
            RetinaFade
        }

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
        }

        void InitializeEyeballBuffer()
        {
            eyeballBuffer = new BufferRgb444(240, 240);

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

        void Delay()
        {
            Thread.Sleep(random.Next(1000, 5000));
        }


        public override async Task Run()
        {
            Console.WriteLine("Run...");

            while (true)
            {
                FadeRetina();
                Delay();
                RandomEyeMovement();
                Delay();
                CenterEye();
                Delay();
            }
        }

        void RandomEyeMovement()
        {
            EyeMovement movementType = (EyeMovement)random.Next(6);

            switch (movementType)
            {
                case EyeMovement.LookLeft:
                    LookLeft();
                    break;
                case EyeMovement.LookRight:
                    LookRight();
                    break;
                case EyeMovement.LookUp:
                    LookUp();
                    break;
                case EyeMovement.LookDown:
                    LookDown();
                    break;
                case EyeMovement.Blink:
                    Blink();
                    break;
                case EyeMovement.Dilate:
                    Dilate();
                    break;
                case EyeMovement.RetinaFade:
                    FadeRetina();
                    break;
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
            Thread.Sleep(100);

            xLast = -1;
            DrawEyeball();

            Thread.Sleep(100);
            graphics.DrawCircle(graphics.Width / 2, graphics.Height / 2, 120, outline, true, true);
            graphics.Show();
            Thread.Sleep(100);

            xLast = -1;
            DrawEyeball();
        }

        readonly int fadeSteps = 20;

        void FadeRetina()
        {
            for (int i = 0; i < fadeSteps; i++)
            {
                DrawRetinaWithFade(i / (double)fadeSteps);
                graphics.Show();
                Thread.Sleep(100);
            }

            Delay();

            for (int i = fadeSteps; i > 0; i--)
            {
                DrawRetinaWithFade(i / (double)fadeSteps);
                graphics.Show();
                Thread.Sleep(100);
            }
        }

        //probably move this to Color as an extension method
        public static Color BlendColors(Color color1, Color color2, double ratio)
        {
            byte r = (byte)(color1.R * (1 - ratio) + color2.R * ratio);
            byte g = (byte)(color1.G * (1 - ratio) + color2.G * ratio);
            byte b = (byte)(color1.B * (1 - ratio) + color2.B * ratio);
            return Color.FromRgb(r, g, b);
        }

        void Dilate()
        {
            DrawEyeball();

            int dilationAmount = 12;

            for (int i = 0; i < dilationAmount; i++)
            {
                graphics.DrawCircle(xLast, yLast, 16 + i, Color.Black, true, true);
                graphics.Show();
            }

            Thread.Sleep(1000);

            for (int i = dilationAmount; i > 0; i--)
            {
                graphics.DrawCircle(xLast, yLast, 32, lightGreen, true, true);
                graphics.DrawCircle(xLast, yLast, 16 + i, Color.Black, true, true);
                graphics.Show();
            }

            DrawEyeball();
        }

        int GetRandomMovementAmount()
        {
            return random.Next(MinEyeMovement, MaxEyeMovement);
        }

        void MoveEye(int xDirection, int yDirection)
        {
            ResetEye();
            var movement = GetRandomMovementAmount();
            for (int i = 0; i < movement; i += MovementStep)
            {
                xOffset = xDirection * i;
                yOffset = yDirection * i;
                DrawEyeball();
            }
        }

        void LookLeft()
        {
            MoveEye(-1, 0);
        }

        void LookRight()
        {
            MoveEye(1, 0);
        }

        void LookUp()
        {
            MoveEye(0, -1);
        }

        void LookDown()
        {
            MoveEye(0, 1);
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

        void DrawEyeball()
        {
            if (xLast == -1)
            {
                graphics.DrawBuffer(graphics.Width / 2 - eyeballBuffer.Width / 2,
                graphics.Height / 2 - eyeballBuffer.Height / 2, eyeballBuffer);
            }
            else
            {
                graphics.DrawRectangle(xLast - 40, yLast - 40, 80, 80, light, true);
            }

            xLast = graphics.Width / 2 + xOffset;
            yLast = graphics.Height / 2 + yOffset;

            graphics.DrawCircle(xLast, yLast, 40, darkGreen, true, true);
            graphics.DrawCircle(xLast, yLast, 32, lightGreen, true, true);

            graphics.DrawCircle(xLast + (xOffset >> 2), yLast + (yOffset >> 2), 16, Color.Black, true, true);

            graphics.Show();
        }

        void DrawRetinaWithFade(double ratio)
        {
            graphics.DrawCircle(xLast, yLast, 40, BlendColors(darkGreen, light, ratio), true, true);
            graphics.DrawCircle(xLast, yLast, 32, BlendColors(lightGreen, light, ratio), true, true);

            graphics.DrawCircle(xLast, yLast, 16, BlendColors(Color.Black, light, ratio), true, true);
        }
    }
}