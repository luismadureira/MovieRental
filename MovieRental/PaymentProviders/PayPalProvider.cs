namespace MovieRental.PaymentProviders
{
    public class PayPalProvider : IPaymentProvider
    {
        public Task<bool> Pay(double price)
        {
            // Dummy implementation
            return Task.FromResult(true);
        }
    }
}
