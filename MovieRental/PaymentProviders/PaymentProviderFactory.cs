using MovieRental.Rental;

namespace MovieRental.PaymentProviders
{
    public class PaymentProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentProvider GetPaymentProvider(PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.mbway => _serviceProvider.GetRequiredService<MbWayProvider>(),
                PaymentMethod.paypal => _serviceProvider.GetRequiredService<PayPalProvider>(),
                _ => throw new InvalidOperationException($"Unsupported payment method: {paymentMethod}")
            };
        }
    }
}