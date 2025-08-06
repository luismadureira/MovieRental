using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.PaymentProviders;

namespace MovieRental.Rental
{
    public class RentalFeatures : IRentalFeatures
    {
        private readonly MovieRentalDbContext _movieRentalDb;
        private readonly PaymentProviderFactory _paymentProviderFactory;

        public RentalFeatures(MovieRentalDbContext movieRentalDb, PaymentProviderFactory paymentProviderFactory)
        {
            _movieRentalDb = movieRentalDb;
            _paymentProviderFactory = paymentProviderFactory;
        }

        /// <summary>
        /// Save a rental to the database asynchronously
        /// </summary>
        /// <param name="rental">The rental object to save</param>
        /// <returns>The saved rental object</returns>
        public async Task<Rental> SaveAsync(Rental rental)
        {
            try
            {
                // Validate rental object here if needed
                if (rental == null)
                {
                    throw new ArgumentNullException(nameof(rental), "Rental cannot be null");
                }

                // Resolve the payment provider
                IPaymentProvider paymentProvider = _paymentProviderFactory.GetPaymentProvider(rental.PaymentMethod);

                // Process payment
                bool paymentSuccess = await paymentProvider.Pay(rental.PaymentValue);

                if (!paymentSuccess)
                {
                    throw new InvalidOperationException("Payment failed");
                }

                // Save rental to the database
                await _movieRentalDb.Rentals.AddAsync(rental);
                await _movieRentalDb.SaveChangesAsync();
                return rental;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to save rental", ex);
            }
        }

        /// <summary>
        /// Get rentals by customer name asynchronously
        /// </summary>
        /// <param name="customerName">The customer name to search for</param>
        /// <returns>A collection of rentals for the specified customer</returns>
        public async Task<IEnumerable<Rental>> GetRentalsByCustomerNameAsync(string customerName)
        {
            try
            {
                return await _movieRentalDb.Rentals
                .Include(r => r.Customer)    // Load Customer entity
                .Include(r => r.Movie)       // Load Movie entity if needed
                .Where(r => r.Customer != null &&
                       r.Customer.Name.ToLower().Contains(customerName.ToLower()))
                .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new InvalidOperationException("Failed to retrieve rentals", ex);
            }
        }

    }
}
