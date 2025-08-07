using Microsoft.EntityFrameworkCore;
using MovieRental.Data;

namespace MovieRental.Movie
{
    public class MovieFeatures : IMovieFeatures
    {
        private readonly MovieRentalDbContext _movieRentalDb;
        public MovieFeatures(MovieRentalDbContext movieRentalDb)
        {
            _movieRentalDb = movieRentalDb;
        }

        /// <summary>
        /// Saves a movie to the database asynchronously.
        /// Note: This method always performs an insert operation and does not handle updates.
        /// If the movie already exists in the database, this will cause a primary key violation.
        /// </summary>
        /// <param name="movie">The movie entity to save to the database.</param>
        /// <returns>The saved movie entity with any database-generated values (like Id).</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to save a movie that already exists in the database.</exception>
        public async Task<Movie> SaveAsync(Movie movie)
        {
            try
            {
                // Validate movie object here if needed
                if (movie == null)
                {
                    throw new ArgumentNullException(nameof(movie), "Movie cannot be null");
                }

                // Add the movie to the database
                await _movieRentalDb.Movies.AddAsync(movie).ConfigureAwait(false);
                await _movieRentalDb.SaveChangesAsync().ConfigureAwait(false);
                return movie;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to save movie", ex);
            }
        }

        /// <summary>
        /// Retrieves movies with pagination to prevent memory issues.
        /// </summary>
        public async Task<IEnumerable<Movie>> GetAllAsync(int pageSize = 100, int pageNumber = 1)
        {
            try
            {
                return await _movieRentalDb.Movies
                    .AsNoTracking() // Prevent EF from tracking entities - saves memory
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to retrieve movies", ex);
            }
        }
    }
}
