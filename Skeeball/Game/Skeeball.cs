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

    public TimeSpan GameTime
    {
        get
        {
            if (CurrentState == GameState.Playing)
            {
                return DateTime.Now - startTime;
            }
            else if (CurrentState == GameState.GameOver)
            {
                return endTime - startTime;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }
    }

    private Dictionary<PlayerPosition, Player> Players;

    private List<int> HighScoresClassic;
    private List<int> HighScoresBonusBall;

    private DateTime gameStartTime;
    private DateTime startTime;
    private DateTime endTime;

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
        startTime = DateTime.Now;

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

    public void NextGameMode()
    {
        CurrentGameMode++;

        if (CurrentGameMode > GameMode.Sequence)
        {
            CurrentGameMode = GameMode.Classic;
        }

        Reset();
    }

    public string GetGameModeDescription(GameMode mode)
    {
        return mode switch
        {
            GameMode.Classic => "Try to get the highest score in 9 balls.",
            GameMode.Bonus => "Try to get the highest score in 9 balls. Scoring 50 awards a bonus ball.",
            GameMode.Timed => "30 seconds to score as many points as possible.",
            GameMode.Exact => "Try to score exactly 250 in the fewest throws.",
            GameMode.Sequence => "Throw a 10, 20, 30, 40, and 50 in order in the fewest throws.",
            _ => $"{mode}"
        };
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
            case GameMode.Bonus:
                ThrowBonusBall(pointValue); break;
            case GameMode.Timed:
                ThrowTimed(pointValue); break;
            case GameMode.Exact:
                ThrowExact(pointValue); break;
            case GameMode.Sequence:
                ThrowSequence(pointValue); break;
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

    private void ThrowTimed(PointValue pointValue)
    {
        Players[CurrentPlayerPosition].ThrowBall(pointValue);

        SwitchTurn();
    }

    private void ThrowExact(PointValue pointValue)
    {
        bool countScore = Players[CurrentPlayerPosition].Score + (int)pointValue <= 250;

        Players[CurrentPlayerPosition].ThrowBall(pointValue, countScore);
        SwitchTurn();
    }

    private void ThrowSequence(PointValue pointValue)
    {
        if (pointValue == PointValue.Ten && Players[CurrentPlayerPosition].Score == 0)
        {
            Players[CurrentPlayerPosition].ThrowBall(pointValue);
        }
        else if (pointValue == PointValue.Twenty && Players[CurrentPlayerPosition].Score == 10)
        {
            Players[CurrentPlayerPosition].ThrowBall(pointValue);
        }
        else if (pointValue == PointValue.Thirty && Players[CurrentPlayerPosition].Score == 30)
        {
            Players[CurrentPlayerPosition].ThrowBall(pointValue);
        }
        else if (pointValue == PointValue.Forty && Players[CurrentPlayerPosition].Score == 60)
        {
            Players[CurrentPlayerPosition].ThrowBall(pointValue);
        }
        else if (pointValue == PointValue.Fifty && Players[CurrentPlayerPosition].Score == 100)
        {
            Players[CurrentPlayerPosition].ThrowBall(pointValue);
        }
        else
        {
            Players[CurrentPlayerPosition].ThrowBall(pointValue, false);
        }

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
            CurrentGameMode == GameMode.Bonus)
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
        if (CurrentGameMode == GameMode.Timed)
        {
            if (GameTime.TotalSeconds >= 30)
            {
                return true;
            }
        }
        if (CurrentGameMode == GameMode.Exact)
        {
            for (int i = 0; i < (int)NumberOfPlayers; i++)
            {
                //check if any players have balls remaining
                if (Players[(PlayerPosition)i].Score == 250)
                {
                    return true;
                }
            }
            return false;
        }
        if (CurrentGameMode == GameMode.Sequence)
        {
            for (int i = 0; i < (int)NumberOfPlayers; i++)
            {
                //check if any players have balls remaining
                if (Players[(PlayerPosition)i].Score == 150)
                {
                    return true;
                }
            }
            return false;
        }

        return true;
    }


    private void EndGame()
    {
        endTime = DateTime.Now;
        for (int i = 0; i < (int)NumberOfPlayers; i++)
        {
            CheckAndAddToHighScores(Players[(PlayerPosition)i].Score);
        }

        CurrentState = GameState.GameOver;
    }

    private void CheckAndAddToHighScores(int score)
    {
        List<int> highScores;

        if (CurrentGameMode == GameMode.Classic)
        {
            highScores = HighScoresClassic;
        }
        else if (CurrentGameMode == GameMode.Bonus)
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