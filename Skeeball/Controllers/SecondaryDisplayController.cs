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

    readonly IPixelBuffer bunny1, bunny2;

    Color ScoreColor => Color.FromHex("#E2B08F");

    public SecondaryDisplayController(IGraphicsDisplay display)
    {
        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font8x12(),
            Rotation = RotationType._180Degrees
        };

        bunny1 = LoadImage("bunny1.jpg");
        bunny2 = LoadImage("bunny2.jpg");
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
        graphics.DrawText(120 + 8, 180, $"{ballsRemaining}", ScoreColor, (ScaleFactor)16, HorizontalAlignment.Center, VerticalAlignment.Center);

        int yStart = (4 - ballsRemaining / 2) * bunny2.Height;

        for (int i = 0; i < ballsRemaining; i++)
        {
            graphics.DrawBuffer(205, yStart + i * bunny2.Height, bunny2);
        }

        graphics.Show();
    }

    public void ShowGameStats(List<SkeeballGame.PointValue> ballScores, int score, int highscore)
    {
        //draw the count of each PointValue in the list along with the player score and current high score, show a custom message if the player has a new high score
        graphics.Clear();

        int y = 24;
        graphics.DrawText(120, y, "Game Stats", Color.White, ScaleFactor.X2, HorizontalAlignment.Center);
        y += 48;
        graphics.DrawText(5, y, "10s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Ten)}", Color.White, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "20s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Twenty)}", Color.White, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "30s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Thirty)}", Color.White, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "40s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Forty)}", Color.White, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, "50s:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{ballScores.Count(x => x == SkeeballGame.PointValue.Fifty)}", Color.White, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 48;
        graphics.DrawText(5, y, $"Score:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{score}", Color.White, ScaleFactor.X2, HorizontalAlignment.Right);
        y += 24;
        graphics.DrawText(5, y, $"High Score:", Color.White, ScaleFactor.X2, HorizontalAlignment.Left);
        graphics.DrawText(235, y, $"{highscore}", Color.White, ScaleFactor.X2, HorizontalAlignment.Right);

        graphics.Show();
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