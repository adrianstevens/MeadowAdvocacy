using HighScoreTracker.Models;
using HighScoreTracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HighScoreTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FastestTimesController : ControllerBase
    {
        private readonly FastestTimeRepository _repository;

        public FastestTimesController(FastestTimeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<List<FastestTime>> GetAll() => _repository.GetAll();

        [HttpPost]
        public ActionResult<FastestTime> Add([FromBody] FastestTime fastestTime)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newFastestTime = _repository.Add(fastestTime);
            return CreatedAtAction(nameof(GetAll), newFastestTime);
        }
    }
}
