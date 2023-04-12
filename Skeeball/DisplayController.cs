using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System.Threading;

namespace Skeeball
{
    public class DisplayController
    {
        MicroGraphics graphics;

        int xOffset = 1;

        public DisplayController(MicroGraphics graphics)
        {
            this.graphics = graphics;
        }

        public void ScrollTextOn(string text, Color color)
        {
            //coded for the 32x8 display
            for (int i = -16; i < 16; i++)
            {
                graphics.Clear();

                graphics.DrawText(xOffset + i * 8, 0, text, color, ScaleFactor.X8, HorizontalAlignment.Center); //x8 for the scale factor on X

                graphics.Show();
            }
        }

        public void DrawText(string text, Color color)
        {
            graphics.Clear();
            graphics.DrawText(xOffset + 16 * 8, 0, text, color, ScaleFactor.X8, HorizontalAlignment.Center); //x8 for the scale factor on X
            graphics.Show();
        }

        public void FlashText(string text, Color color1, Color color2)
        {
            for (int i = 0; i < 6; i++)
            {
                DrawText(text, color1);
                DrawText(text, color2);
            }
        }

        public void DrawTitle()
        {
            char[] letters = "SKEEBALL".ToCharArray();

            Color colorBase = Color.White;
            Color colorOverlay = Color.Cyan;

            void DrawTitleColor(Color color)
            {
                graphics.Clear();
                graphics.DrawText(xOffset, 0, "SKEEBALL", color, ScaleFactor.X8);
            }

            graphics.Clear();

            for (int i = 0; i < 8; i++)
            {
                graphics.DrawText(xOffset + i * 32, 0, $"{letters[i]}", colorBase, ScaleFactor.X8);
                graphics.Show();
                Thread.Sleep(50);
            }

            for (int i = 0; i < 8; i++)
            {
                DrawTitleColor(colorBase);
                graphics.DrawText(xOffset + i * 32, 0, $"{letters[i]}", colorOverlay, ScaleFactor.X8);
                graphics.Show();
            }

            DrawTitleColor(colorBase);
            graphics.Show();
            Thread.Sleep(50);

            DrawTitleColor(colorOverlay);
            graphics.Show();
        }

        public void DrawPointsAwarded(int points, int totalScore)
        {
            graphics.Clear();
            graphics.DrawText(2, 0, "SKEEBALL", Color.Red, ScaleFactor.X8);
            graphics.Show();
        }
    }
}
