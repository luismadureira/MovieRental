using Microsoft.EntityFrameworkCore;
using MovieRental.Data;

namespace MovieRental.Customer
{
    public class CustomerFeatures : ICustomerFeatures
    {
        private readonly MovieRentalDbContext _movieRentalDb;

        public CustomerFeatures(MovieRentalDbContext movieRentalDb)
        {
            _movieRentalDb = movieRentalDb ?? throw new ArgumentNullException(nameof(movieRentalDb));
        }

        /// <summary>
        /// Saves or updates a customer in the database asynchronously.
        /// Automatically determines whether to insert or update based on Id value.
        /// </summary>
        /// <param name="customer">The customer entity to save to the database.</param>
        /// <returns>The saved customer entity with any database-generated values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when customer is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the save operation fails.</exception>
        public async Task<Customer> SaveAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null");

            try
            {
                if (customer.Id == 0)
                {
                    // Validate email uniqueness for new customers
                    if (!string.IsNullOrWhiteSpace(customer.Email))
                    {
                        Customer? existingCustomer = await GetByEmailAsync(customer.Email).ConfigureAwait(false);
                        if (existingCustomer != null)
                            throw new InvalidOperationException($"A customer with email '{customer.Email}' already exists");
                    }

                    await _movieRentalDb.Customers.AddAsync(customer).ConfigureAwait(false);
                }
                else
                {
                    _movieRentalDb.Customers.Update(customer);
                }

                await _movieRentalDb.SaveChangesAsync().ConfigureAwait(false);
                return customer;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException("Failed to save customer", ex);
            }
        }

        /// <summary>
        /// Retrieves customers with pagination to prevent memory issues.
        /// </summary>
        public async Task<IEnumerable<Customer>> GetAllAsync(int pageSize = 100, int pageNumber = 1)
        {
            if (pageSize <= 0) pageSize = 100;
            if (pageNumber <= 0) pageNumber = 1;

            try
            {
                return await _movieRentalDb.Customers
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(c => c.Name)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve customers", ex);
            }
        }

        /// <summary>
        /// Retrieves a customer by their email address.
        /// </summary>
        public async Task<Customer?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                return await _movieRentalDb.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower())
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve customer with email {email}", ex);
            }
        }
    }
}
