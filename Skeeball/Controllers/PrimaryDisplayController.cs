using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System.Threading;
using System.Threading.Tasks;

namespace Skeeball.Controllers;

internal class PrimaryDisplayController
{
    readonly MicroGraphics graphics;

    readonly int xOffset = 0;

    public PrimaryDisplayController(IGraphicsDisplay display)
    {
        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font4x8(),
        };
    }

    public void ScrollTextOn(string text, Color color)
    {
        //coded for the 32x8 display
        for (int i = -16; i < 16; i++)
        {
            graphics.Clear();

            graphics.DrawText(xOffset + i, 0, text, color, ScaleFactor.X1, HorizontalAlignment.Center);
            graphics.Show();
            Thread.Sleep(100);
        }
    }

    //Draws text centered on the Apa102 display
    public void DrawText(string text, Color color)
    {
        graphics.Clear();
        graphics.DrawText(xOffset + 16, 0, text, color, ScaleFactor.X1, HorizontalAlignment.Center); //x8 for the scale factor on X
        graphics.Show();
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
            graphics.Clear();
            graphics.DrawText(xOffset, 0, "SKEEBALL", color);
        }

        graphics.Clear();

        for (int i = 0; i < 8; i++)
        {
            graphics.DrawText(xOffset + i, 0, $"{letters[i]}", colorBase);
            graphics.Show();
            Thread.Sleep(50);
        }

        for (int i = 0; i < 8; i++)
        {
            DrawTitleColor(colorBase);
            graphics.DrawText(xOffset + i, 0, $"{letters[i]}", colorOverlay);
            graphics.Show();
        }

        DrawTitleColor(colorBase);
        graphics.Show();
        Thread.Sleep(50);

        DrawTitleColor(colorOverlay);
        graphics.Show();
    }

    public async Task ShowEndGame(int totalScore)
    {
        FlashText($"GAMEOVER", Color.Red, Color.Yellow);
        DrawText("GAMEOVER", Color.Red);
        await Task.Delay(2000);
        ScrollTextOn("YOUR SCORE:", Color.Red);
        await Task.Delay(200);
        FlashText($"{totalScore}", Color.LawnGreen, Color.Cyan);
    }

    public void AwardPoints(int points, int totalScore)
    {
        FlashText($"{points}", Color.Blue, Color.Violet);
        DrawText($"{totalScore}", Color.White);
    }
}