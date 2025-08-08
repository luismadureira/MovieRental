namespace MovieRental.Customer;

public interface ICustomerFeatures
{
    Task<Customer> SaveAsync(Customer costumer);
    Task<IEnumerable<Customer>> GetAllAsync(int pageSize = 100, int pageNumber = 1);
}