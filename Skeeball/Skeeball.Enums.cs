﻿namespace Skeeball
{
    public partial class SkeeballGame
    {
        public enum PointValue
        {
            Ten = 10,
            Twenty = 20,
            Thirty = 30,
            Forty = 40,
            Fifty = 50
        }

        public enum GameMode
        {
            Classic,
            BonusBall,
            Doubles,
            TimeAttack
        }

        public enum PlayerPosition
        {
            One = 1,
            Two,
            Three,
            Four,
        }

        public enum GameState
        {
            Initializing,
            ReadyToStart,
            Playing,
            GameOver
        }
    }
}