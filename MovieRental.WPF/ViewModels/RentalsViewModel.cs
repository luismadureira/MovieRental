using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MovieRental.WPF.ViewModels
{
    public class RentalsViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _disposed = false;
        private int _daysRented;
        private double _paymentValue;
        private string _customerToSearch = string.Empty;
        private string _statusMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private Models.PaymentMethod _selectedPaymentMethod;
        private Models.Customer? _selectedCustomer;
        private Models.Movie? _selectedMovie;
        #endregion

        #region Collections
        public ObservableCollection<Models.Rental> Rentals { get; set; } = new();
        public ObservableCollection<Models.Customer> Customers { get; set; } = new();
        public ObservableCollection<Models.Movie> Movies { get; set; } = new();
        public IEnumerable<Models.PaymentMethod> PaymentMethods => Enum.GetValues(typeof(Models.PaymentMethod)).Cast<Models.PaymentMethod>();
        #endregion

        #region Properties
        public int DaysRented
        {
            get => _daysRented;
            set { _daysRented = value; OnPropertyChanged(); }
        }

        public double PaymentValue
        {
            get => _paymentValue;
            set { _paymentValue = value; OnPropertyChanged(); }
        }

        public string CustomerToSearch
        {
            get => _customerToSearch;
            set { _customerToSearch = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public Models.PaymentMethod SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set { _selectedPaymentMethod = value; OnPropertyChanged(); }
        }

        public Models.Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        public Models.Movie? SelectedMovie
        {
            get => _selectedMovie;
            set { _selectedMovie = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand SearchCommand { get; }
        #endregion

        #region Constructor
        public RentalsViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveRentalAsync());
            SearchCommand = new RelayCommand(async () => await SearchRentalAsync());
            _ = LoadMoviesAsync();
            _ = LoadCustomersAsync();
        }
        #endregion

        #region Public Methods
        public async Task SaveRentalAsync()
        {
            try
            {
                if (SelectedCustomer == null || SelectedMovie == null)
                {
                    StatusMessage = "Please select both customer and movie";
                    return;
                }

                if (PaymentValue <= 0)
                {
                    StatusMessage = "Payment value field is required.";
                    return;
                }

                if (DaysRented <= 0)
                {
                    StatusMessage = "Days rented value field is required.";
                    return;
                }

                Models.Rental newRental = new Models.Rental
                {
                    DaysRented = DaysRented,
                    CustomerId = SelectedCustomer.Id,
                    MovieId = SelectedMovie.Id,
                    PaymentMethod = SelectedPaymentMethod,
                    PaymentValue = PaymentValue
                };

                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "rental"), newRental)
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Rental added!";
                    ClearForm();
                }
                else
                {
                    StatusMessage = $"Save failed: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        #endregion

        #region Private Methods
        private async Task SearchRentalAsync()
        {
            try
            {
                if (String.IsNullOrEmpty(CustomerToSearch))
                {
                    StatusMessage = "Customer name field is required.";
                    return;
                }

                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                using HttpResponseMessage response = await _httpClient.GetAsync(
                    new Uri(new Uri(baseUrl), $"rental?customerName={Uri.EscapeDataString(CustomerToSearch)}"));

                List<Models.Rental>? rentals = await response.Content.ReadFromJsonAsync<List<Models.Rental>>();

                Rentals.Clear();
                if (rentals != null)
                {
                    foreach (Models.Rental rental in rentals)
                        Rentals.Add(rental);
                }
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
                using HttpResponseMessage response = await _httpClient.GetAsync(new Uri(new Uri(baseUrl), "movie"));

                List<Models.Movie>? movies = await response.Content.ReadFromJsonAsync<List<Models.Movie>>();

                Movies.Clear();
                if (movies != null)
                {
                    foreach (Models.Movie movie in movies)
                        Movies.Add(movie);

                    SelectedMovie = Movies.FirstOrDefault();
                }
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
                using HttpResponseMessage response = await _httpClient.GetAsync(new Uri(new Uri(baseUrl), "customer"));

                List<Models.Customer>? customers = await response.Content.ReadFromJsonAsync<List<Models.Customer>>();

                Customers.Clear();
                if (customers != null)
                {
                    foreach (Models.Customer customer in customers)
                        Customers.Add(customer);

                    SelectedCustomer = Customers.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            DaysRented = 0;
            PaymentValue = 0;
            SelectedMovie = Movies.FirstOrDefault();
            SelectedCustomer = Customers.FirstOrDefault();
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
        #endregion
    }
}
