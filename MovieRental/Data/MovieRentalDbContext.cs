using Microsoft.EntityFrameworkCore;

namespace MovieRental.Data
{
    public class MovieRentalDbContext : DbContext
    {
        public DbSet<Movie.Movie> Movies { get; set; }
        public DbSet<Rental.Rental> Rentals { get; set; }
        public DbSet<Customer.Customer> Customers { get; set; }

        private string DbPath { get; }

        public MovieRentalDbContext()
        {
            Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
            string path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "movierental.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
