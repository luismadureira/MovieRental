namespace MovieRental.WPF.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int DaysRented { get; set; }
        public int MovieId { get; set; }
        public Models.Movie? Movie { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public double PaymentValue { get; set; }
        public int CustomerId { get; set; }
        public Models.Customer? Customer { get; set; }
    }

    public enum PaymentMethod
    {
        mbway,
        paypal
    }
}
