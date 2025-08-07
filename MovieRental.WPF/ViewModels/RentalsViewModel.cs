using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MovieRental.WPF.ViewModels
{
    public class RentalsViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields
        private static readonly HttpClient s_httpClient = CreateHttpClient();
        private bool _disposed = false;
        private int _daysRented;
        private double _paymentValue;
        private string _customerToSearch = string.Empty;
        private string _statusMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
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
            set { _daysRented = value; OnPropertyChanged(); ValidateForm(); }
        }

        public double PaymentValue
        {
            get => _paymentValue;
            set { _paymentValue = value; OnPropertyChanged(); ValidateForm(); }
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

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public Models.PaymentMethod SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set { _selectedPaymentMethod = value; OnPropertyChanged(); ValidateForm(); }
        }

        public Models.Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); ValidateForm(); }
        }

        public Models.Movie? SelectedMovie
        {
            get => _selectedMovie;
            set { _selectedMovie = value; OnPropertyChanged(); ValidateForm(); }
        }

        public bool IsFormValid => SelectedCustomer != null && SelectedMovie != null && PaymentValue > 0 && DaysRented > 0;
        public bool IsSearchValid => !string.IsNullOrWhiteSpace(CustomerToSearch);
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand SearchCommand { get; }
        #endregion

        #region Constructor
        public RentalsViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveRentalAsync().ConfigureAwait(false), () => IsFormValid && !IsLoading);
            SearchCommand = new RelayCommand(async () => await SearchRentalAsync().ConfigureAwait(false), () => IsSearchValid && !IsLoading);

            _ = Task.Run(async () =>
            {
                try
                {
                    await LoadMoviesAsync().ConfigureAwait(false);
                    await LoadCustomersAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ErrorMessage = $"Failed to load initial data: {ex.Message}";
                    });
                }
            });
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Add("User-Agent", "MovieRental-WPF/1.0");
            return client;
        }
        #endregion

        #region Public Methods
        public async Task SaveRentalAsync()
        {
            if (!IsFormValid) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                Models.Rental newRental = new Models.Rental
                {
                    DaysRented = DaysRented,
                    CustomerId = SelectedCustomer!.Id,
                    MovieId = SelectedMovie!.Id,
                    PaymentMethod = SelectedPaymentMethod,
                    PaymentValue = PaymentValue
                };

                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ApiBaseUrl not configured");

                using HttpResponseMessage response = await s_httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "rental"), newRental)
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Rental added successfully!";
                    ClearForm();
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorMessage = $"Save failed: {response.StatusCode}. {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Network error: {ex.Message}";
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                ErrorMessage = "Request timed out. Please try again.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchRentalAsync()
        {
            if (!IsSearchValid) return;

            IsLoading = true;
            ErrorMessage = string.Empty;
            StatusMessage = string.Empty;

            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ApiBaseUrl not configured");

                using HttpResponseMessage response = await s_httpClient.GetAsync(
                    new Uri(new Uri(baseUrl), $"rental?customerName={Uri.EscapeDataString(CustomerToSearch.Trim())}"))
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    List<Models.Rental>? rentals = await response.Content.ReadFromJsonAsync<List<Models.Rental>>()
                        .ConfigureAwait(false);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Rentals.Clear();
                        if (rentals != null)
                        {
                            foreach (Models.Rental? rental in rentals.OrderByDescending(r => r.Id))
                                Rentals.Add(rental);
                        }

                        StatusMessage = rentals?.Count > 0
                            ? $"Found {rentals.Count} rental(s) for '{CustomerToSearch}'"
                            : $"No rentals found for '{CustomerToSearch}'";
                    });
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorMessage = $"Search failed: {response.StatusCode}. {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Network error: {ex.Message}";
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                ErrorMessage = "Request timed out. Please try again.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error searching rentals: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Private Methods
        private async Task LoadMoviesAsync()
        {
            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ApiBaseUrl not configured");

                using HttpResponseMessage response = await s_httpClient.GetAsync(new Uri(new Uri(baseUrl), "movie"))
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    List<Models.Movie>? movies = await response.Content.ReadFromJsonAsync<List<Models.Movie>>()
                        .ConfigureAwait(false);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Models.Movie? currentSelection = SelectedMovie;
                        Movies.Clear();

                        if (movies != null)
                        {
                            foreach (Models.Movie? movie in movies.OrderBy(m => m.Title))
                                Movies.Add(movie);

                            // Restore selection if possible, otherwise select first
                            SelectedMovie = currentSelection != null && Movies.Any(m => m.Id == currentSelection.Id)
                                ? Movies.First(m => m.Id == currentSelection.Id)
                                : Movies.FirstOrDefault();
                        }
                    });
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new InvalidOperationException($"Failed to load movies: {response.StatusCode}. {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Network error loading movies: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new InvalidOperationException("Request timed out while loading movies", ex);
            }
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ApiBaseUrl not configured");

                using HttpResponseMessage response = await s_httpClient.GetAsync(new Uri(new Uri(baseUrl), "customer"))
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    List<Models.Customer>? customers = await response.Content.ReadFromJsonAsync<List<Models.Customer>>()
                        .ConfigureAwait(false);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Models.Customer? currentSelection = SelectedCustomer;
                        Customers.Clear();

                        if (customers != null)
                        {
                            foreach (Models.Customer? customer in customers.OrderBy(c => c.Name))
                                Customers.Add(customer);

                            // Restore selection if possible, otherwise select first
                            SelectedCustomer = currentSelection != null && Customers.Any(c => c.Id == currentSelection.Id)
                                ? Customers.First(c => c.Id == currentSelection.Id)
                                : Customers.FirstOrDefault();
                        }
                    });
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new InvalidOperationException($"Failed to load customers: {response.StatusCode}. {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Network error loading customers: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new InvalidOperationException("Request timed out while loading customers", ex);
            }
        }

        private void ClearForm()
        {
            DaysRented = 0;
            PaymentValue = 0;
            CustomerToSearch = string.Empty;
            SelectedMovie = Movies.FirstOrDefault();
            SelectedCustomer = Customers.FirstOrDefault();
            StatusMessage = string.Empty;
            ErrorMessage = string.Empty;

            // Clear rentals display
            Rentals.Clear();
        }

        private void ValidateForm()
        {
            OnPropertyChanged(nameof(IsFormValid));
            OnPropertyChanged(nameof(IsSearchValid));
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
                // Static HttpClient not disposed here
            }
            _disposed = true;
        }
        #endregion
    }
}
