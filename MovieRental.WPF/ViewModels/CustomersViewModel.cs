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
    public class CustomersViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields
        private static readonly HttpClient s_httpClient = CreateHttpClient();
        private bool _disposed = false;
        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _phone = string.Empty;
        private string _statusMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading = false;
        private Models.Customer? _selectedCustomer;
        #endregion

        #region Properties
        public ObservableCollection<Models.Customer> Customers { get; set; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); ValidateForm(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); ValidateForm(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); ValidateForm(); }
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

        public Models.Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                LoadCustomerForEdit();
            }
        }

        public bool IsFormValid => !string.IsNullOrWhiteSpace(Name);
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        #endregion

        #region Constructor
        public CustomersViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveCustomerAsync().ConfigureAwait(false), () => IsFormValid && !IsLoading);

            _ = Task.Run(async () =>
            {
                try
                {
                    await LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Failed to load initial data: {ex.Message}";
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
        public async Task SaveCustomerAsync()
        {
            if (!IsFormValid) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                Models.Customer customer = new Models.Customer
                {
                    Id = SelectedCustomer?.Id ?? 0,
                    Name = Name.Trim(),
                    Email = Email.Trim(),
                    Phone = Phone.Trim()
                };

                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ApiBaseUrl not configured");

                HttpResponseMessage result;

                if (customer.Id == 0)
                {
                    result = await s_httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "customer"), customer)
                        .ConfigureAwait(false);
                }
                else
                {
                    result = await s_httpClient.PutAsJsonAsync(new Uri(new Uri(baseUrl), $"customer/{customer.Id}"), customer)
                        .ConfigureAwait(false);
                }

                using (result)
                {
                    if (result.IsSuccessStatusCode)
                    {
                        StatusMessage = customer.Id == 0 ? "Customer added!" : "Customer updated!";
                        ClearForm();
                        await LoadCustomersAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        string errorContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                        ErrorMessage = $"Save failed: {result.StatusCode}. {errorContent}";
                    }
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
        #endregion

        #region Private Methods
        private async Task LoadCustomersAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ApiBaseUrl not configured");

                using HttpResponseMessage response = await s_httpClient.GetAsync(new Uri(new Uri(baseUrl), "customer"));

                if (response.IsSuccessStatusCode)
                {
                    List<Models.Customer>? customers = await response.Content.ReadFromJsonAsync<List<Models.Customer>>();

                    if (customers != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Customers.Clear();
                            foreach (Models.Customer? customer in customers.OrderBy(c => c.Name))
                                Customers.Add(customer);
                        });
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorMessage = $"Failed to load customers: {response.StatusCode}. {errorContent}";
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
                ErrorMessage = $"Error loading customers: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadCustomerForEdit()
        {
            if (SelectedCustomer != null)
            {
                Name = SelectedCustomer.Name;
                Email = SelectedCustomer.Email ?? string.Empty;
                Phone = SelectedCustomer.Phone ?? string.Empty;
            }
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            SelectedCustomer = null;
            StatusMessage = string.Empty;
            ErrorMessage = string.Empty;
        }

        private void ValidateForm()
        {
            OnPropertyChanged(nameof(IsFormValid));
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
