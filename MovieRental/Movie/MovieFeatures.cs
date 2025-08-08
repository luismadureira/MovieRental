using Microsoft.EntityFrameworkCore;
using MovieRental.Data;

namespace MovieRental.Movie
{
    public class MovieFeatures : IMovieFeatures
    {
        private readonly MovieRentalDbContext _movieRentalDb;

        public MovieFeatures(MovieRentalDbContext movieRentalDb)
        {
            _movieRentalDb = movieRentalDb ?? throw new ArgumentNullException(nameof(movieRentalDb));
        }

        /// <summary>
        /// Saves or updates a movie in the database asynchronously.
        /// Automatically determines whether to insert or update based on Id value.
        /// </summary>
        /// <param name="movie">The movie entity to save to the database.</param>
        /// <returns>The saved movie entity with any database-generated values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when movie is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the save operation fails.</exception>
        public async Task<Movie> SaveAsync(Movie movie)
        {
            if (movie == null)
                throw new ArgumentNullException(nameof(movie), "Movie cannot be null");

            if (string.IsNullOrWhiteSpace(movie.Title))
                throw new ArgumentException("Movie title cannot be empty", nameof(movie));

            try
            {
                if (movie.Id == 0)
                {
                    await _movieRentalDb.Movies.AddAsync(movie).ConfigureAwait(false);
                }
                else
                {
                    // Use Attach and SetModified for better tracking
                    Movie? existingMovie = await _movieRentalDb.Movies
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == movie.Id)
                        .ConfigureAwait(false);

                    if (existingMovie == null)
                        throw new InvalidOperationException($"Movie with ID {movie.Id} not found");

                    _movieRentalDb.Entry(movie).State = EntityState.Modified;
                }

                await _movieRentalDb.SaveChangesAsync().ConfigureAwait(false);

                // Detach entity to prevent tracking issues
                _movieRentalDb.Entry(movie).State = EntityState.Detached;

                return movie;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save movie", ex);
            }
        }

        /// <summary>
        /// Retrieves movies with pagination to prevent memory issues.
        /// </summary>
        public async Task<IEnumerable<Movie>> GetAllAsync(int pageSize = 100, int pageNumber = 1)
        {
            if (pageSize <= 0) pageSize = 100;
            if (pageNumber <= 0) pageNumber = 1;

            try
            {
                return await _movieRentalDb.Movies
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(m => m.Title)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve movies", ex);
            }
        }
    }
}
