using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace Skeeball
{
    public class DisplayController
    {
        MicroGraphics graphics;

        public DisplayController(MicroGraphics graphics)
        {
            this.graphics = graphics;
        }

        public void DrawTitle()
        {
            char[] letters = "SKEEBALL".ToCharArray();
            int xOffset = 2;

            Color colorBase = Color.White;
            Color colorOverlay = Color.Cyan;

            void DrawTitleColor(Color color)
            {
                graphics.Clear();
                graphics.DrawText(xOffset, 0, "SKEEBALL", color, ScaleFactor.X8);
                DrawState();
            }


            DrawTitleColor(colorBase);
            graphics.Show();


            for (int i = 0; i < 8; i++)
            {
                DrawTitleColor(colorBase);
                graphics.DrawText(xOffset + i * 32, 0, $"{letters[i]}", colorOverlay, ScaleFactor.X8);
                graphics.Show();
            }

            DrawTitleColor(colorBase);
            graphics.Show();

            DrawTitleColor(colorOverlay);
            graphics.Show();
        }

        public void DrawPointsAwarded(int points, int totalScore)
        {
            graphics.Clear();

            graphics.DrawText(2, 0, "SKEEBALL", Color.Red, ScaleFactor.X8);

            DrawState();

            graphics.Show();
        }

        void DrawState()
        {
            graphics.DrawText(2, 60, "Current Player", Color.White, ScaleFactor.X2);
            graphics.DrawText(2, 80, "Score", Color.White, ScaleFactor.X2);
            graphics.DrawText(2, 100, "Balls remaining", Color.White, ScaleFactor.X3);

        }
    }
}
