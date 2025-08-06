using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieRental.Controllers;
using MovieRental.Movie;

namespace MovieRental.Test.Unit
{
    public class MovieControllerTests
    {
        private readonly Mock<IMovieFeatures> _mockMovieFeatures;
        private readonly MovieController _controller;

        public MovieControllerTests()
        {
            _mockMovieFeatures = new Mock<IMovieFeatures>();
            _controller = new MovieController(_mockMovieFeatures.Object);
        }

        [Fact]
        public async Task Post_ShouldReturnCreatedStatus_WhenMovieIsSaved()
        {
            // Arrange
            Movie.Movie movie = new Movie.Movie
            {
                Id = 1,
                Title = "Inception"
            };
            _mockMovieFeatures.Setup(f => f.SaveAsync(movie)).ReturnsAsync(movie);

            // Act
            IActionResult result = await _controller.Post(movie);

            // Assert
            ObjectResult createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(movie, createdResult.Value);
        }

        [Fact]
        public async Task Get_ShouldReturnOkStatus_WithListOfMovies()
        {
            // Arrange
            List<Movie.Movie> movies = new List<Movie.Movie>
            {
                new Movie.Movie { Id = 1, Title = "Inception" },
                new Movie.Movie { Id = 2, Title = "The Matrix" }
            };
            _mockMovieFeatures.Setup(f => f.GetAllAsync()).ReturnsAsync(movies);

            // Act
            IActionResult result = await _controller.Get();

            // Assert
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(movies, okResult.Value);
        }
    }
}