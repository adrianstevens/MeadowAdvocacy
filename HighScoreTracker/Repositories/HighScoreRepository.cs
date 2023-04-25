using HighScoreTracker.Models;

namespace HighScoreTracker.Repositories
{
    public class HighScoreRepository
    {
        private readonly List<HighScore> _highScores = new();
        private int _nextId = 1;

        public List<HighScore> GetAll() => _highScores.OrderByDescending(h => h.Score).ToList();

        public HighScore Add(HighScore highScore)
        {
            highScore.Id = _nextId++;
            _highScores.Add(highScore);
            return highScore;
        }
    }
}