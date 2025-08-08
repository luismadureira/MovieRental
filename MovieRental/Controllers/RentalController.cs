using Microsoft.AspNetCore.Mvc;
using MovieRental.Rental;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RentalController : ControllerBase
    {

        private readonly IRentalFeatures _features;

        public RentalController(IRentalFeatures features)
        {
            _features = features;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Rental.Rental))]
        public async Task<IActionResult> Post([FromBody] Rental.Rental rental)
        {
            return StatusCode(StatusCodes.Status201Created, await _features.SaveAsync(rental));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Rental.Rental))]
        public async Task<IActionResult> Get([FromQuery] string customerName)
        {
            return StatusCode(StatusCodes.Status200OK, await _features.GetRentalsByCustomerNameAsync(customerName));
        }

    }
}
