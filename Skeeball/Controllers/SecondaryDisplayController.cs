using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using SimpleJpegDecoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Skeeball.Controllers;

internal class SecondaryDisplayController
{
    readonly MicroGraphics graphics;

    readonly IPixelBuffer bunny1, bunny2, carrot1, boston1;

    readonly Random random = new();

    Color ScoreColor => Color.FromHex("#E2B08F");

    public SecondaryDisplayController(IGraphicsDisplay display)
    {
        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font8x12(),
            Rotation = RotationType.Default
        };

        bunny1 = LoadImage("bunny1.jpg");
        bunny2 = LoadImage("bunny2.jpg");
        carrot1 = LoadImage("carrot1.jpg");
        boston1 = LoadImage("boston1.jpg");
    }

    public void Clear()
    {
        graphics.Clear(true);
    }

    public void ShowSplash()
    {
        graphics.Clear();

        graphics.DrawBuffer(graphics.Width / 2 - bunny1.Width / 2, graphics.Height / 2 - bunny1.Height / 2, bunny1);

        //draw the text BunnyBall below the image of the bunny
        graphics.DrawText(120, 200, "BunnyBall", Color.White, ScaleFactor.X2, HorizontalAlignment.Center);

        graphics.Show();
    }

    public void ShowBallsRemaining(int ballsRemaining)
    {
        graphics.Clear();
        graphics.DrawText(120 + 8, 130, $"{ballsRemaining}", ScoreColor, (ScaleFactor)16, HorizontalAlignment.Center, VerticalAlignment.Center);

        int yStart = (4 - ballsRemaining / 2) * bunny2.Height;

        if (ballsRemaining > 5)
        {
            for (int i = 0; i < ballsRemaining - 5; i++)
            {
                graphics.DrawBuffer(31 + i * 48, 232, bunny2);
            }
        }

        if (ballsRemaining > 0)
        {
            for (int i = 0; i < ballsRemaining; i++)
            {
                graphics.DrawBuffer(7 + i * 48, 280, bunny2);
            }
        }

        if (ballsRemaining == 0)
        {
            var x = random.Next(0, graphics.Width - boston1.Width);
            graphics.DrawBuffer(x, 320 - boston1.Height, boston1);
        }

        graphics.Show();
    }

    public void ShowGameDescription(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(description.ToUpper()))
            return;

        graphics.Clear();

        graphics.DrawText(120, 40, title, Color.White, ScaleFactor.X2, HorizontalAlignment.Center);

        int y = 100;
        int ySpacing = 28;
        while (description.Length > 15)
        {
            //find the first space before the 15th character
            int spaceIndex = description.Substring(0, 14).LastIndexOf(' ');

            //draw the text up to the spaceIndex and remove it from the string
            graphics.DrawText(120, y, description[..spaceIndex], Color.White, ScaleFactor.X2, HorizontalAlignment.Center);
            y += ySpacing;
            description = description.Substring(spaceIndex + 1);
        }
        graphics.DrawText(120, y, description, Color.White, ScaleFactor.X2, HorizontalAlignment.Center);

        graphics.Show();
    }

    public void ShowGameStats(List<SkeeballGame.PointValue> ballScores, int score, int highscore, TimeSpan gameTime)
    {
        //draw the count of each PointValue in the list along with the player score and current high score, show a custom message if the player has a new high score
        graphics.Clear();

        graphics.DrawBuffer(graphics.Width / 2 - carrot1.Width / 2, graphics.Height / 2 - carrot1.Height / 2, carrot1);

        int y = 24;
        graphics.DrawText(120, y, "Game Stats", Color.White, ScaleFactor.X2, HorizontalAlignment.Center);
        y += 48;
        graphics.DrawText(5, y, "10s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Ten)}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "20s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Twenty)}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "30s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Thirty)}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "40s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Forty)}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "50s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Fifty)}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "Time:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{gameTime:mm\\:ss}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 48;
        graphics.DrawText(5, y, $"Your Score:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{score}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, $"High Score:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{highscore}", Color.LawnGreen, ScaleFactor.X2, HorizontalAlignment.Right);

        graphics.Show();
    }

    IPixelBuffer LoadImage(string name)
    {
        var jpgData = LoadResource(name);

        var decoder = new JpegDecoder();
        var jpg = decoder.DecodeJpeg(jpgData);

        return new BufferRgb888(decoder.Width, decoder.Height, jpg).ConvertPixelBuffer<BufferRgb565>();
    }

    byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Skeeball.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}