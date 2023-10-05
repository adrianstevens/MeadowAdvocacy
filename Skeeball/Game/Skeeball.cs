using System;
using System.Collections.Generic;

namespace Skeeball;

public partial class SkeeballGame
{
    public GameMode CurrentGameMode { get; private set; } = GameMode.Classic;

    public GameState CurrentState { get; private set; } = GameState.Initializing;

    public Player CurrentPlayer => Players[CurrentPlayerPosition];

    public PlayerPosition CurrentPlayerPosition { get; private set; }

    public PlayerPosition NumberOfPlayers { get; private set; }

    private Dictionary<PlayerPosition, Player> Players;

    private List<int> HighScoresClassic;
    private List<int> HighScoresBonusBall;

    private DateTime _gameStartTime;
    private DateTime _startTime;

    private readonly int MAX_HIGHSCORES = 10;

    public SkeeballGame()
    {
        Players = new Dictionary<PlayerPosition, Player>
        {
            { PlayerPosition.One, new Player() { Position = PlayerPosition.One } },
            { PlayerPosition.Two, new Player() { Position = PlayerPosition.Two } },
            { PlayerPosition.Three, new Player() { Position = PlayerPosition.Three } },
            { PlayerPosition.Four, new Player() { Position = PlayerPosition.Four } }
        };

        HighScoresClassic = new List<int>();

        HighScoresBonusBall = new List<int>();

        Reset();
    }

    public void Reset()
    {
        Console.WriteLine("Skeeball Reset");

        CurrentState = GameState.ReadyToStart;
        CurrentPlayerPosition = PlayerPosition.One;
        _startTime = DateTime.Now;

        foreach (var player in Players)
        {
            player.Value.Reset();
        }
    }

    public void StartGame(GameMode gameMode, PlayerPosition players)
    {
        CurrentGameMode = gameMode;
        NumberOfPlayers = players;
        StartGame();
    }

    public bool StartGame()
    {
        if (CurrentState == GameState.Playing || CurrentState == GameState.Initializing)
        {
            Console.WriteLine($"Skeeball StartGame false {CurrentState}");
            return false;
        }

        Console.WriteLine($"Skeeball StartGame true {CurrentState}");

        Reset();
        CurrentState = GameState.Playing;
        return true;
    }

    public int GetHighscore()
    {
        return 450;
    }

    public int GetScore(PlayerPosition player)
    {
        if (player > NumberOfPlayers)
        {
            return 0;
        }
        return Players[player].Score;
    }

    public int GetBallsRemaining(PlayerPosition player)
    {
        if (player > NumberOfPlayers)
        {
            return 0;
        }
        return Players[player].BallsRemaining;
    }

    public bool ThrowBall(PointValue pointValue)
    {
        Console.WriteLine($"ThrowBall {pointValue} {CurrentState} {CurrentGameMode} {CurrentPlayerPosition} {CurrentPlayer.BallsRemaining} {CurrentPlayer.Score}");

        if (CurrentState != GameState.Playing)
        {
            return false;
        }

        switch (CurrentGameMode)
        {
            case GameMode.Classic:
                ThrowBallClassic(pointValue); break;
            case GameMode.BonusBall:
                ThrowBonusBall(pointValue); break;
            case GameMode.TimeAttack:
                ThrowTimeAttack(pointValue); break;
            case GameMode.PerfectScore:
                ThrowPerfectScore(pointValue); break;
            case GameMode.CompleteSet:
                ThrowCompleteSet(pointValue); break;
        }

        return true;
    }

    private void ThrowBallClassic(PointValue pointValue)
    {
        Players[CurrentPlayerPosition].ThrowBall(pointValue);

        SwitchTurn();
    }

    private void ThrowBonusBall(PointValue pointValue)
    {
        Players[CurrentPlayerPosition].ThrowBall(pointValue);

        if (pointValue == PointValue.Fifty)
        {
            Players[CurrentPlayerPosition].AddBonusBall();
        }

        SwitchTurn();
    }

    private void ThrowTimeAttack(PointValue pointValue)
    {
        Players[CurrentPlayerPosition].ThrowBall(pointValue);

        SwitchTurn();
    }

    private void ThrowPerfectScore(PointValue pointValue)
    {
        bool countScore = Players[CurrentPlayerPosition].Score + (int)pointValue <= 250;

        Players[CurrentPlayerPosition].ThrowBall(pointValue, countScore);
        SwitchTurn();
    }

    private void ThrowCompleteSet(PointValue pointValue)
    {
        Players[CurrentPlayerPosition].ThrowBall(pointValue);

        SwitchTurn();
    }

    private void SwitchTurn()
    {
        if (NumberOfPlayers == PlayerPosition.One)
        {
            if (IsGameOver())
            {
                EndGame();
            }
            else
            {
                return;
            }
        }

        CurrentPlayerPosition++;

        if (CurrentPlayerPosition > NumberOfPlayers)
        {
            CurrentPlayerPosition = PlayerPosition.One;
        }

        if (CurrentPlayer.BallsRemaining == 0)
        {
            if (IsGameOver())
            {
                EndGame();
            }
            else
            {
                SwitchTurn();
            }
        }
    }

    private bool IsGameOver()
    {
        if (CurrentGameMode == GameMode.Classic ||
            CurrentGameMode == GameMode.BonusBall)
        {
            for (int i = 0; i < (int)NumberOfPlayers; i++)
            {
                //check if any players have balls remaining
                if (Players[(PlayerPosition)i].BallsRemaining > 0)
                {
                    return false;
                }
            }
            return true;
        }
        return true;
    }


    private void EndGame()
    {
        for (int i = 0; i < (int)NumberOfPlayers; i++)
        {
            AddScore(Players[(PlayerPosition)i].Score);
        }

        CurrentState = GameState.GameOver;
    }

    private void AddScore(int score)
    {
        List<int> highScores;

        if (CurrentGameMode == GameMode.Classic)
        {
            highScores = HighScoresClassic;
        }
        else if (CurrentGameMode == GameMode.BonusBall)
        {
            highScores = HighScoresBonusBall;
        }
        else
        {
            throw new NotSupportedException();
        }

        highScores.Add(score);

        if (highScores.Count > MAX_HIGHSCORES)
        {
            highScores.Sort();
            highScores.Reverse();
            highScores.RemoveAt(MAX_HIGHSCORES);
        }
    }

    public void ResetHighScores()
    {
        HighScoresClassic.Clear();
        HighScoresBonusBall.Clear();
    }
}