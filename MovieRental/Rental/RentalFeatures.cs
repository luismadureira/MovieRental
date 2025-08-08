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
            _movieRentalDb = movieRentalDb ?? throw new ArgumentNullException(nameof(movieRentalDb));
            _paymentProviderFactory = paymentProviderFactory ?? throw new ArgumentNullException(nameof(paymentProviderFactory));
        }

        /// <summary>
        /// Saves a rental to the database asynchronously with payment processing.
        /// </summary>
        public async Task<Rental> SaveAsync(Rental rental)
        {
            if (rental == null)
                throw new ArgumentNullException(nameof(rental), "Rental cannot be null");

            if (rental.PaymentValue <= 0)
                throw new ArgumentException("Payment value must be greater than zero", nameof(rental));

            try
            {
                // Resolve the payment provider
                IPaymentProvider paymentProvider = _paymentProviderFactory.GetPaymentProvider(rental.PaymentMethod);

                // Process payment
                bool paymentSuccess = await paymentProvider.Pay(rental.PaymentValue).ConfigureAwait(false);

                if (!paymentSuccess)
                    throw new InvalidOperationException("Payment failed");

                // Save rental to the database
                await _movieRentalDb.Rentals.AddAsync(rental).ConfigureAwait(false);
                await _movieRentalDb.SaveChangesAsync().ConfigureAwait(false);
                return rental;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException) && !(ex is ArgumentException))
            {
                throw new InvalidOperationException("Failed to save rental", ex);
            }
        }

        /// <summary>
        /// Gets rentals by customer name asynchronously with proper navigation properties.
        /// </summary>
        public async Task<IEnumerable<Rental>> GetRentalsByCustomerNameAsync(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                return Enumerable.Empty<Rental>();

            try
            {
                return await _movieRentalDb.Rentals
                    .Include(r => r.Customer)
                    .Include(r => r.Movie)
                    .AsNoTracking()
                    .Where(r => r.Customer != null &&
                               r.Customer.Name.ToLower().Contains(customerName.ToLower()))
                    .OrderByDescending(r => r.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve rentals by customer name", ex);
            }
        }
    }
}
