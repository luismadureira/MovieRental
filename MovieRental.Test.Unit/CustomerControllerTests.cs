using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieRental.Controllers;
using MovieRental.Customer;

namespace MovieRental.Test.Unit
{
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerFeatures> _mockCustomerFeatures;
        private readonly CustomerController _controller;

        public CustomerControllerTests()
        {
            _mockCustomerFeatures = new Mock<ICustomerFeatures>();
            _controller = new CustomerController(_mockCustomerFeatures.Object);
        }

        [Fact]
        public async Task Post_ShouldReturnCreatedStatus_WhenCustomerIsSaved()
        {
            // Arrange
            Customer.Customer customer = new Customer.Customer
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com"
            };
            _mockCustomerFeatures.Setup(f => f.SaveAsync(customer)).ReturnsAsync(customer);

            // Act
            IActionResult result = await _controller.Post(customer);

            // Assert
            ObjectResult createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(customer, createdResult.Value);
        }

        [Fact]
        public async Task Get_ShouldReturnOkStatus_WithListOfCustomers()
        {
            // Arrange
            List<Customer.Customer> customers = new List<Customer.Customer>
            {
                new Customer.Customer { Id = 1, Name = "John Doe", Email = "john.doe@example.com" },
                new Customer.Customer { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com" }
            };
            _mockCustomerFeatures.Setup(f => f.GetAllAsync(100, 1)).ReturnsAsync(customers);

            // Act
            IActionResult result = await _controller.Get();

            // Assert
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(customers, okResult.Value);
        }
    }
}