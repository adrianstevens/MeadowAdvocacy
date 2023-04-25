using HighScoreTracker.Models;
using HighScoreTracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HighScoreTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HighScoresController : ControllerBase
    {
        private readonly HighScoreRepository _repository;

        public HighScoresController(HighScoreRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<List<HighScore>> GetAll() => _repository.GetAll();

        [HttpPost]
        public ActionResult<HighScore> Add([FromBody] HighScore highScore)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newHighScore = _repository.Add(highScore);
            return CreatedAtAction(nameof(GetAll), newHighScore);
        }
    }
}
