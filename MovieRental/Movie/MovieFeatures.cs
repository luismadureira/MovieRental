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

        // <summary>
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
                await _movieRentalDb.Movies.AddAsync(movie);
                await _movieRentalDb.SaveChangesAsync();
                return movie;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to save movie", ex);
            }
        }

        // TODO: tell us what is wrong in this method? Forget about the async, what other concerns do you have?
        //Potential issues with this method:
        //- No handling for null reference issues. For example we have no validation that _movieRentalDb or _movieRentalDb.Movies exists.
        //- Loading ALL movies without filtering/pagination into memory at once could cause memory issues.
        //With thousands of movies, this could cause OutOfMemoryException.

        /// <summary>
        /// Retrieves all movies from the database asynchronously.
        /// </summary>
        /// <returns>A list of all movies in the database.</returns>
        /// <exception cref="InvalidOperationException">Thrown when there are database connectivity issues.</exception>
        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            try
            {
                return await _movieRentalDb.Movies.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to retrieve movies", ex);
            }
        }


    }
}
