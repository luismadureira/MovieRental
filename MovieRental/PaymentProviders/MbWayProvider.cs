namespace MovieRental.PaymentProviders
{
    public class MbWayProvider : IPaymentProvider
    {
        public Task<bool> Pay(double price)
        {
            // Dummy implementation
            return Task.FromResult(true);
        }
    }
}
