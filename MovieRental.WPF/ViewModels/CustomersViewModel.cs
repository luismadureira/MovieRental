using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MovieRental.WPF.ViewModels
{
    public class CustomersViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public ObservableCollection<Models.Customer> Customers { get; set; } = new();

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string StatusMessage { get; set; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public CustomersViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveCustomerAsync());
            _ = LoadCustomersAsync();
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
            }
        }

        public async Task SaveCustomerAsync()
        {
            try
            {
                Models.Customer newCustomer = new Models.Customer { Name = Name, Email = Email, Phone = Phone };
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                HttpResponseMessage result = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "customer"), newCustomer);

                if (result.IsSuccessStatusCode)
                {
                    StatusMessage = "Customer added!";
                    Name = string.Empty;
                    Email = string.Empty;
                    Phone = string.Empty;
                    await LoadCustomersAsync();
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
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(Phone));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
