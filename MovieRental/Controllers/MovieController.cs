using Microsoft.AspNetCore.Mvc;
using MovieRental.Movie;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieController : ControllerBase
    {

        private readonly IMovieFeatures _features;

        public MovieController(IMovieFeatures features)
        {
            _features = features;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Movie.Movie))]
        public async Task<IActionResult> Post([FromBody] Movie.Movie movie)
        {
            return StatusCode(StatusCodes.Status201Created, await _features.SaveAsync(movie));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Movie.Movie))]
        public async Task<IActionResult> Get()
        {
            return StatusCode(StatusCodes.Status200OK, await _features.GetAllAsync());
        }
    }
}
