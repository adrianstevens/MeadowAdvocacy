using System;
using System.Collections.Generic;

namespace Skeeball
{
    public partial class SkeeballGame
    {
        public GameMode CurrentGameMode { get; private set; }

        public GameState CurrentState { get; private set; }

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
                { PlayerPosition.One, new Player() },
                { PlayerPosition.Two, new Player() },
                { PlayerPosition.Three, new Player() },
                { PlayerPosition.Four, new Player() }
            };

            HighScoresClassic = new List<int>();

            HighScoresBonusBall = new List<int>();

            Reset();
        }

        void Reset()
        {
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

        public void StartGame()
        {
            Reset();
            CurrentState = GameState.Playing;
        }

        public void ThrowBall(PointValue pointValue)
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            switch (CurrentGameMode)
            {
                case GameMode.Classic:
                    ThrowBallClassic(pointValue); break;
                case GameMode.BonusBall:
                    ThrowBonusBall(pointValue); break;
            }
        }

        private void ThrowBallClassic(PointValue pointValue)
        {
            Players[CurrentPlayerPosition].Score += (int)pointValue;

            SwitchTurn();
        }

        private void ThrowBonusBall(PointValue pointValue)
        {
            Players[CurrentPlayerPosition].Score += (int)pointValue;

            if (pointValue == PointValue.Fifty)
            {
                Players[CurrentPlayerPosition].BallsRemaining += (int)pointValue;
            }

            SwitchTurn();
        }

        private void SwitchTurn()
        {
            if (NumberOfPlayers == PlayerPosition.One)
            {
                return;
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
}