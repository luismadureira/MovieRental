using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MovieRental.WPF.ViewModels
{
    public class RentalsViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public ObservableCollection<Models.Rental> Rentals { get; set; } = new();
        public ObservableCollection<Models.Customer> Customers { get; set; } = new();
        public ObservableCollection<Models.Movie> Movies { get; set; } = new();
        public IEnumerable<Models.PaymentMethod> PaymentMethods => Enum.GetValues(typeof(Models.PaymentMethod)).Cast<Models.PaymentMethod>();

        public int DaysRented { get; set; }
        public double PaymentValue { get; set; }
        public string CustomerToSearch { get; set; }

        private Models.PaymentMethod _selectedPaymentMethod;
        public Models.PaymentMethod SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged(nameof(SelectedPaymentMethod));
            }
        }

        private Models.Customer _selectedCustomer;
        public Models.Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
            }
        }

        private Models.Movie _selectedMovie;
        public Models.Movie SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged(nameof(SelectedMovie));
            }
        }


        public string StatusMessage { get; set; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public ICommand SearchCommand { get; }

        public RentalsViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveRentalAsync());
            SearchCommand = new RelayCommand(async () => await SearchRentalAsync());
            _ = LoadMoviesAsync();
            _ = LoadCustomersAsync();
        }

        private async Task SearchRentalAsync()
        {
            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                List<Models.Rental>? response = await _httpClient.GetFromJsonAsync<List<Models.Rental>>(new Uri(new Uri(baseUrl), $"rental?customerName={Uri.EscapeDataString(CustomerToSearch)}"));

                Rentals.Clear();
                foreach (Models.Rental rental in response)
                    Rentals.Add(rental);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading rentals: {ex.Message}";
            }
        }

        private async Task LoadMoviesAsync()
        {
            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                List<Models.Movie>? response = await _httpClient.GetFromJsonAsync<List<Models.Movie>>(new Uri(new Uri(baseUrl), "movie"));

                Movies.Clear();
                foreach (Models.Movie movie in response)
                    Movies.Add(movie);

                SelectedMovie = Movies.FirstOrDefault();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading movies: {ex.Message}";
            }
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                List<Models.Customer>? response = await _httpClient.GetFromJsonAsync<List<Models.Customer>>(new Uri(new Uri(baseUrl), "customer"));

                Customers.Clear();
                foreach (Models.Customer customer in response)
                    Customers.Add(customer);

                SelectedCustomer = Customers.FirstOrDefault();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
            }
        }

        public async Task SaveRentalAsync()
        {
            try
            {
                Models.Rental newRental = new Models.Rental
                {
                    DaysRented = DaysRented,
                    CustomerId = SelectedCustomer.Id,
                    MovieId = SelectedMovie.Id,
                    PaymentMethod = SelectedPaymentMethod,
                    PaymentValue = PaymentValue
                };
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                HttpResponseMessage result = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "rental"), newRental);

                if (result.IsSuccessStatusCode)
                {
                    StatusMessage = "Rental added!";
                    DaysRented = 0;
                    PaymentValue = 0;
                    SelectedMovie = Movies.FirstOrDefault();
                    SelectedCustomer = Customers.FirstOrDefault();
                }
                else
                {
                    StatusMessage = $"Save failed: {result.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }

            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(DaysRented));
            OnPropertyChanged(nameof(PaymentValue));
            OnPropertyChanged(nameof(SelectedCustomer));
            OnPropertyChanged(nameof(SelectedMovie));
            OnPropertyChanged(nameof(SelectedPaymentMethod));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
