using Meadow;
using Meadow.Foundation.Graphics;
using System.Threading;
using System.Threading.Tasks;

namespace Skeeball.Controllers;

internal class PrimaryDisplayService
{
    readonly MicroGraphics graphics;

    readonly int xOffset = 0;

    const double brightness = 0.01;

    readonly Color DisplayPink = Color.Pink.WithBrightness(brightness);
    readonly Color DisplayWhite = Color.White.WithBrightness(brightness);
    readonly Color DisplayCyan = Color.Cyan.WithBrightness(brightness);
    readonly Color DisplayLawnGreen = Color.LawnGreen.WithBrightness(brightness);
    readonly Color DisplayYellow = Color.Yellow.WithBrightness(brightness);
    readonly Color DisplayOrange = Color.Orange.WithBrightness(brightness);
    readonly Color DisplayRed = Color.Red.WithBrightness(brightness);
    readonly Color DisplayPurple = Color.Purple.WithBrightness(brightness);
    readonly Color DisplayViolet = Color.Violet.WithBrightness(brightness);
    readonly Color DisplayBlue = Color.Blue.WithBrightness(brightness);

    readonly IFont fontText;
    readonly IFont fontNumber;

    public PrimaryDisplayService(IGraphicsDisplay display)
    {
        graphics = new MicroGraphics(display);
        fontText = new Font4x8();
        fontNumber = new Font6x8();
    }

    public void ShowGameMode(SkeeballGame.GameMode mode)
    {
        DrawText($"{mode}".ToUpper(), DisplayWhite);
    }

    public void ShowReady()
    {
        DrawText("READY", DisplayWhite);
    }

    public void ShowTitle()
    {
        char[] letters = "SKEEBALL".ToCharArray();

        void DrawTitleColor(Color color)
        {
            graphics.Clear();
            graphics.DrawText(xOffset, 1, "SKEEBALL", color, font: fontText);
        }

        void AnimateIn(Color color)
        {
            for (int i = 0; i < 8; i++)
            {
                graphics.DrawText(xOffset + i * 4, 1, $"{letters[i]}", color, font: fontText);
                graphics.Show();
                Thread.Sleep(100);
            }
        }

        graphics.Clear();

        AnimateIn(DisplayBlue);
        AnimateIn(DisplayCyan);
        AnimateIn(DisplayWhite);
        AnimateIn(DisplayYellow);
        AnimateIn(DisplayLawnGreen);
    }

    public async Task ShowEndGame(int totalScore)
    {
        FlashText($"GAMEOVER", DisplayRed, DisplayYellow);
        DrawText("GAMEOVER", DisplayRed);
        await Task.Delay(2000);
        ScrollTextOn("YOUR SCORE:", DisplayRed);
        await Task.Delay(200);
        FlashText($"{totalScore}", DisplayCyan, DisplayLawnGreen);
    }

    public void AwardPoints(int points, int totalScore)
    {
        Color color1, color2;

        switch (points)
        {
            case 10:
                color1 = DisplayCyan;
                color2 = DisplayBlue;
                break;
            case 20:
                color1 = DisplayRed;
                color2 = DisplayOrange;
                break;
            case 30:
                color1 = DisplayViolet;
                color2 = DisplayYellow;
                break;
            case 40:
                color1 = DisplayLawnGreen;
                color2 = DisplayPurple;
                break;
            case 50:
            default:
                color1 = DisplayPink;
                color2 = DisplayWhite;
                break;
        }

        FlashText($"{points}", color1, color2);
        DrawText($"{totalScore}", DisplayWhite);
    }

    void ScrollTextOn(string text, Color color)
    {
        //coded for the 32x8 display
        for (int i = 32; i > 0; i--)
        {
            graphics.Clear();

            graphics.DrawText(xOffset + i, 1, text, color, ScaleFactor.X1, HorizontalAlignment.Center, font: fontText);
            graphics.Show();
            Thread.Sleep(100);
        }
    }

    //Draws text centered on the Apa102 display
    void DrawText(string text, Color color)
    {
        int len = text.Length;

        graphics.Clear();
        graphics.DrawText(xOffset + 16, 1, text, color, ScaleFactor.X1, HorizontalAlignment.Center, font: len <= 5 ? fontNumber : fontText);
        graphics.Show();
    }

    //Flashes text on the Apa102 display
    void FlashText(string text, Color color1, Color color2)
    {
        for (int i = 0; i < 8; i++)
        {
            DrawText(text, color1);
            Thread.Sleep(50);
            DrawText(text, color2);
            Thread.Sleep(50);
        }
    }
}