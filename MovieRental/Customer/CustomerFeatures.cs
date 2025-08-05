using Microsoft.EntityFrameworkCore;
using MovieRental.Data;

namespace MovieRental.Customer
{
    public class CustomerFeatures : ICustomerFeatures
    {
        private readonly MovieRentalDbContext _movieRentalDb;
        public CustomerFeatures(MovieRentalDbContext movieRentalDb)
        {
            _movieRentalDb = movieRentalDb;
        }

        /// <summary>
        /// Save a customer to the database asynchronously
        /// </summary>
        /// <param name="customer">The customer object to save</param>
        /// <returns>The saved customer object</returns>
        public async Task<Customer> SaveAsync(Customer costumer)
        {
            try
            {
                // Validate costumer object here if needed
                if (costumer == null)
                {
                    throw new ArgumentNullException(nameof(costumer), "Customer cannot be null");
                }

                // Add the costumer to the database
                await _movieRentalDb.Customers.AddAsync(costumer);
                await _movieRentalDb.SaveChangesAsync();
                return costumer;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to save customer", ex);
            }
        }

        /// <summary>
        /// Get all customers asynchronously
        /// </summary>
        /// <returns>A collection of customers</returns>
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            try
            {
                return await _movieRentalDb.Customers.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to retrieve customers", ex);
            }
        }

    }
}
