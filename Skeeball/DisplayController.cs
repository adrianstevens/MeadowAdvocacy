using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System.Threading;

namespace Skeeball
{
    public class DisplayController
    {
        readonly MicroGraphics graphicsTop;
        readonly MicroGraphics graphicsBottom;

        readonly int xOffset = 0;

        public DisplayController(MicroGraphics graphicsTop, MicroGraphics graphicsBottom)
        {
            this.graphicsTop = graphicsTop;
            this.graphicsBottom = graphicsBottom;
        }

        public void ScrollTextOn(string text, Color color)
        {
            //coded for the 32x8 display
            for (int i = -16; i < 16; i++)
            {
                graphicsTop.Clear();

                graphicsTop.DrawText(xOffset + i, 0, text, color, ScaleFactor.X1, HorizontalAlignment.Center);
                graphicsTop.Show();
                Thread.Sleep(100);
            }
        }

        //Draws text centered on the Apa102 display
        public void DrawText(string text, Color color)
        {
            graphicsTop.Clear();
            graphicsTop.DrawText(xOffset + 16, 0, text, color, ScaleFactor.X1, HorizontalAlignment.Center); //x8 for the scale factor on X
            graphicsTop.Show();
        }

        //Flashes text on the Apa102 display
        public void FlashText(string text, Color color1, Color color2)
        {
            for (int i = 0; i < 6; i++)
            {
                DrawText(text, color1);
                Thread.Sleep(50);
                DrawText(text, color2);
                Thread.Sleep(50);
            }
        }

        public void DrawTitle()
        {
            char[] letters = "SKEEBALL".ToCharArray();

            Color colorBase = Color.White;
            Color colorOverlay = Color.Cyan;

            void DrawTitleColor(Color color)
            {
                graphicsTop.Clear();
                graphicsTop.DrawText(xOffset, 0, "SKEEBALL", color);
            }

            graphicsTop.Clear();

            for (int i = 0; i < 8; i++)
            {
                graphicsTop.DrawText(xOffset + i, 0, $"{letters[i]}", colorBase);
                graphicsTop.Show();
                Thread.Sleep(50);
            }

            for (int i = 0; i < 8; i++)
            {
                DrawTitleColor(colorBase);
                graphicsTop.DrawText(xOffset + i, 0, $"{letters[i]}", colorOverlay);
                graphicsTop.Show();
            }

            DrawTitleColor(colorBase);
            graphicsTop.Show();
            Thread.Sleep(50);

            DrawTitleColor(colorOverlay);
            graphicsTop.Show();
        }

        public void DrawPointsAwarded(int points, int totalScore)
        {
            graphicsTop.Clear();
            graphicsTop.DrawText(2, 0, "SKEEBALL", Color.Red);
            graphicsTop.Show();
        }
    }
}