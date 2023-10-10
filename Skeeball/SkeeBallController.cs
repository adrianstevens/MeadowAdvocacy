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
    LEDController leds;

    SkeeballGame game;

    readonly Random random = new();

    public async Task Initialize()
    {
        hardware = new SkeeballHardware();

        await hardware.Initialize();

        secondaryDisplay = new SecondaryDisplayController(hardware.BottomDisplay);
        secondaryDisplay.Clear();
        primaryDisplay = new PrimaryDisplayController(hardware.TopDisplay);
        audio = new AudioController(hardware.Speaker);
        leds = new LEDController();

        hardware.StartButton.Clicked += StartButton_Clicked;
        hardware.StartButton.LongClicked += StartButton_LongClicked;
        hardware.SelectButton.Clicked += SelectButton_Clicked;
        hardware.Score10Switch.Clicked += Score10_Clicked;
        hardware.Score50Switch.Clicked += Score50_Clicked;

        game = new SkeeballGame();

        Console.WriteLine("Init complete");
    }

    private void SelectButton_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("SelectButton_Clicked");

        game.NextGameMode();
        primaryDisplay.ShowGameMode(game.CurrentGameMode);
        secondaryDisplay.ShowGameDescription($"{game.CurrentGameMode}", game.GetGameModeDescription(game.CurrentGameMode));
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

    private void Score10_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("Score10_Clicked");

        var randomValue = random.Next(1, 6) * 10;

        _ = ThrowBall((SkeeballGame.PointValue)randomValue);
    }

    private void Score50_Clicked(object sender, EventArgs e)
    {
        Console.WriteLine("Score50_Clicked");

        _ = ThrowBall(SkeeballGame.PointValue.Fifty);
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
            secondaryDisplay.ShowGameStats(game.CurrentPlayer.BallScores, game.CurrentPlayer.Score, game.GetHighscore(), game.GameTime);
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