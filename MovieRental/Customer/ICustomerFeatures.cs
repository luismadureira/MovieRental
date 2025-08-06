namespace MovieRental.Customer;

public interface ICustomerFeatures
{
    Task<Customer> SaveAsync(Customer costumer);
    Task<IEnumerable<Customer>> GetAllAsync();
}