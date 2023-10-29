using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;
using System.Threading;

namespace JuegoEyeball
{
    public class EyeballController
    {
        readonly MicroGraphics graphics;

        IPixelBuffer eyeballBuffer;

        Color outline = Color.FromHex("241E25");

        Color red = Color.FromHex("F27274");
        Color pink = Color.FromHex("F9C0C5");
        Color light = Color.FromHex("FFEADD");

        Color darkGreen = Color.FromHex("367D17");
        Color lightGreen = Color.FromHex("55B835");

        int xOffset = 0;
        int yOffset = 0;

        int xLast = -1;
        int yLast = -1;

        readonly int MovementStep = 4;
        readonly int ReturnStep = 12;

        readonly int MinEyeMovement = 16;
        readonly int MaxEyeMovement = 50;

        readonly int FadeSteps = 16;

        readonly Random random = new Random();

        public enum EyeMovement
        {
            LookLeft,
            LookRight,
            LookUp,
            LookDown,
            Blink,
            Dilate,
            RetinaFade,
            Count
        }

        public EyeballController(IGraphicsDisplay display)
        {
            graphics = new MicroGraphics(display);

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

        public void Delay()
        {
            Thread.Sleep(random.Next(1000, 5000));
        }

        public void RandomEyeMovement()
        {
            EyeMovement movementType = (EyeMovement)random.Next((int)EyeMovement.Count);

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

        void FadeRetina()
        {
            double step = 1 / (double)FadeSteps;

            for (int i = 0; i < FadeSteps; i++)
            {
                DrawRetinaWithFade(1 - i * step);
                graphics.Show();
                Thread.Sleep(100);
            }

            Delay();

            for (int i = 0; i < FadeSteps; i++)
            {
                DrawRetinaWithFade(i * step);
                graphics.Show();
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Blend two colors together
        /// </summary>
        /// <param name="color1">Color 1</param>
        /// <param name="color2">Color 2</param>
        /// <param name="ratio">The ratio or percentage of color 2</param>
        /// <returns></returns>
        public static Color BlendColors(Color color1, Color color2, double ratio)
        {
            if (ratio == 0)
            {
                return color1;
            }
            if (ratio == 1)
            {
                return color2;
            }

            byte r = (byte)(color1.R * (1 - ratio) + color2.R * ratio);
            byte g = (byte)(color1.G * (1 - ratio) + color2.G * ratio);
            byte b = (byte)(color1.B * (1 - ratio) + color2.B * ratio);
            return Color.FromRgb(r, g, b);
        }

        void Dilate()
        {
            DrawEyeball();

            int dilationAmount = 10;

            for (int i = -2; i < dilationAmount; i++)
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

        public void CenterEye()
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

        public void DrawEyeball()
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