using Microsoft.AspNetCore.Mvc;
using MovieRental.Customer;

namespace MovieRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {

        private readonly ICustomerFeatures _features;

        public CustomerController(ICustomerFeatures features)
        {
            _features = features;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Customer.Customer))]
        public async Task<IActionResult> Post([FromBody] Customer.Customer customer)
        {
            return StatusCode(StatusCodes.Status201Created, await _features.SaveAsync(customer));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Customer.Customer))]
        public async Task<IActionResult> Get()
        {
            return StatusCode(StatusCodes.Status200OK, await _features.GetAllAsync());
        }
    }
}
