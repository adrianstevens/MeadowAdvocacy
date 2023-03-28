namespace Skeeball
{
    public class Player
    {
        public int BallsRemaining { get; set; }
        public int Score { get; set; }

        public SkeeballGame.PlayerPosition Position { get; set; }

        private static readonly int DefaultBallsPerGame = 9;

        public Player()
        {
            Reset();
        }

        public void Reset()
        {
            BallsRemaining = DefaultBallsPerGame;
            Score = 0;
        }
    }
}