namespace MovieRental.Rental;

public interface IRentalFeatures
{
    Task<Rental> SaveAsync(Rental rental);
    Task<IEnumerable<Rental>> GetRentalsByCustomerNameAsync(string customerName);
}