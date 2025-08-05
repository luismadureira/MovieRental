namespace MovieRental.Movie;

public interface IMovieFeatures
{
    Task<Movie> SaveAsync(Movie movie);
    Task<IEnumerable<Movie>> GetAllAsync();
}