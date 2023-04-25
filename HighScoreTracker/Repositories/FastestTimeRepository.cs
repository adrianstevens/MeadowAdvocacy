using HighScoreTracker.Models;

namespace HighScoreTracker.Repositories
{
    public class FastestTimeRepository
    {
        private readonly List<FastestTime> _fastestTimes = new();
        private int _nextId = 1;

        public List<FastestTime> GetAll() => _fastestTimes.OrderBy(t => t.Time).ToList();

        public FastestTime Add(FastestTime fastestTime)
        {
            fastestTime.Id = _nextId++;
            _fastestTimes.Add(fastestTime);
            return fastestTime;
        }
    }
}