using Skeeball.Controllers;
using System;
using System.Threading.Tasks;

namespace Skeeball;

internal class SkeeBallController
{
    SkeeballHardware hardware;
    PrimaryDisplayController primaryDisplay;
    SecondaryDisplayController secondaryDisplay;
    AudioController audio;

    SkeeballGame game;

    readonly Random random = new();

    public async Task Initialize()
    {
        hardware = new SkeeballHardware();

        await hardware.Initialize();

        var projLab = hardware.ProjLab;

        secondaryDisplay = new SecondaryDisplayController(projLab.Display);
        secondaryDisplay.Clear();
        primaryDisplay = new PrimaryDisplayController(hardware.TopDisplay);
        audio = new AudioController(projLab.Speaker);

        hardware.StartButton.Clicked += StartButton_Clicked;
        hardware.StartButton.LongClicked += StartButton_LongClicked;
        hardware.SelectButton.Clicked += SelectButton_Clicked;
        projLab.LeftButton.Clicked += LeftButton_Clicked;

        game = new SkeeballGame();

        Console.WriteLine("Init complete");
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
            primaryDisplay.ShowReady();
            secondaryDisplay.ShowBallsRemaining(game.CurrentPlayer.BallsRemaining);
        }
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
        game.Reset();

        primaryDisplay.ShowTitle();
        secondaryDisplay.ShowSplash();

        return Task.CompletedTask;
    }
}