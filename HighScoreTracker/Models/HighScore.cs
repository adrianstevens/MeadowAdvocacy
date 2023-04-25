namespace HighScoreTracker.Models
{
    public class HighScore
    {
        public int Id { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public double TimePlayed { get; set; }
        public int Deaths { get; set; }
    }
}