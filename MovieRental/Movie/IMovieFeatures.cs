namespace MovieRental.Movie;

public interface IMovieFeatures
{
    Task<Movie> SaveAsync(Movie movie);
    Task<IEnumerable<Movie>> GetAllAsync(int pageSize = 100, int pageNumber = 1);
}