using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Skeeball.Controllers;
using System;
using System.Threading.Tasks;

namespace Skeeball;

internal class SkeeBallController
{
    SkeeballGame game;

    //hardware
    IProjectLabHardware projLab;
    Apa102 topDisplay;

    //controllers
    PrimaryDisplayController primaryDisplay;
    SecondaryDisplayController secondaryDisplay;
    AudioController audio;

    readonly Random random = new();

    public Task Initialize()
    {
        Console.WriteLine("Initialize...");

        projLab = ProjectLab.Create();

        projLab.DownButton.Clicked += StartButton_Clicked;
        projLab.UpButton.Clicked += SelectButton_Clicked;
        projLab.LeftButton.Clicked += LeftButton_Clicked;
        projLab.RightButton.Clicked += RightButton_Clicked;

        audio = new AudioController(projLab.Speaker);

        secondaryDisplay = new SecondaryDisplayController(projLab.Display);

        topDisplay = new Apa102(projLab.MikroBus1.SpiBus, 32, 8);
        primaryDisplay = new PrimaryDisplayController(topDisplay);

        game = new SkeeballGame();

        Console.WriteLine("Init complete");
        return Task.CompletedTask;
    }

    private void SelectButton_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("SelectButton_Clicked");
    }

    private void StartButton_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("StartButton_Clicked");

        audio.PlayThemeSong();

        if (game.StartGame())
        {
            primaryDisplay.DrawText("READY", Color.LawnGreen);
            secondaryDisplay.ShowBallsRemaining(game.CurrentPlayer.BallsRemaining);
        }
    }

    private void RightButton_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("RightButton_Clicked");
    }

    private void StartButton_LongClicked(object sender, EventArgs e)
    {
        Console.WriteLine("StartButton_LongClicked");
        game.Reset();
    }

    private void LeftButton_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("LeftButton_Clicked");

        var randomValue = random.Next(1, 6) * 10;

        _ = ThrowBall((SkeeballGame.PointValue)randomValue);
    }

    async Task ThrowBall(SkeeballGame.PointValue pointValue)
    {
        if (!game.ThrowBall(pointValue))
        {
            return;
        }

        primaryDisplay.AwardPoints((int)pointValue, game.CurrentPlayer.Score);
        secondaryDisplay.ShowBallsRemaining(game.CurrentPlayer.BallsRemaining);

        if (game.CurrentState == SkeeballGame.GameState.GameOver)
        {
            await primaryDisplay.ShowEndGame(game.CurrentPlayer.Score);
            secondaryDisplay.ShowGameStats(game.CurrentPlayer.BallScores, game.CurrentPlayer.Score, game.GetHighscore());
        }
    }

    public Task Run()
    {
        _ = projLab.RgbLed.StartBlink(Color.LawnGreen, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000), 0.5f);

        primaryDisplay.DrawTitle();
        game.Reset();

        secondaryDisplay.ShowSplash();

        return Task.CompletedTask;
    }
}