using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieRental.Controllers;
using MovieRental.Rental;

namespace MovieRental.Test.Unit
{
    public class RentalControllerTests
    {
        private readonly Mock<IRentalFeatures> _mockRentalFeatures;
        private readonly RentalController _controller;

        public RentalControllerTests()
        {
            _mockRentalFeatures = new Mock<IRentalFeatures>();
            _controller = new RentalController(_mockRentalFeatures.Object);
        }

        [Fact]
        public async Task Post_ShouldReturnCreatedStatus_WhenRentalIsSaved()
        {
            // Arrange
            Rental.Rental rental = new Rental.Rental
            {
                Id = 1,
                DaysRented = 5,
                PaymentMethod = PaymentMethod.mbway,
                MovieId = 101,
                CustomerId = 202
            };
            _mockRentalFeatures.Setup(f => f.SaveAsync(rental)).ReturnsAsync(rental);

            // Act
            IActionResult result = await _controller.Post(rental);

            // Assert
            ObjectResult createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(rental, createdResult.Value);
        }

        [Fact]
        public async Task Get_ShouldReturnOkStatus_WithListOfRentals()
        {
            // Arrange
            string customerName = "John Doe";
            List<Rental.Rental> rentals = new List<Rental.Rental>
            {
                new Rental.Rental { Id = 1, DaysRented = 5, PaymentMethod = PaymentMethod.paypal, MovieId = 101, CustomerId = 202 },
                new Rental.Rental { Id = 2, DaysRented = 3, PaymentMethod = PaymentMethod.mbway, MovieId = 102, CustomerId = 203 }
            };
            _mockRentalFeatures.Setup(f => f.GetRentalsByCustomerNameAsync(customerName)).ReturnsAsync(rentals);

            // Act
            IActionResult result = await _controller.Get(customerName);

            // Assert
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(rentals, okResult.Value);
        }
    }
}