using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieRental.Rental
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }
        public int DaysRented { get; set; }

        [ForeignKey("Movie")]
        public int MovieId { get; set; }
        public Movie.Movie? Movie { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public double PaymentValue { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public Customer.Customer? Customer { get; set; }
    }

    public enum PaymentMethod
    {
        mbway,
        paypal
    }
}
