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
        #region Fields
        private readonly HttpClient _httpClient = new HttpClient();
        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _phone = string.Empty;
        private string _statusMessage = string.Empty;
        private string _errorMessage = string.Empty;
        #endregion

        #region Properties
        public ObservableCollection<Models.Customer> Customers { get; set; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
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
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        #endregion

        #region Constructor
        public CustomersViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveCustomerAsync());
            _ = LoadCustomersAsync();
        }
        #endregion

        #region Public Methods
        public async Task SaveCustomerAsync()
        {
            try
            {
                if (Name == string.Empty)
                {
                    StatusMessage = "Name field is required.";
                    return;
                }

                Models.Customer newCustomer = new Models.Customer { Name = Name, Email = Email, Phone = Phone };
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                HttpResponseMessage result = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "customer"), newCustomer);

                if (result.IsSuccessStatusCode)
                {
                    StatusMessage = "Customer added!";
                    ClearForm();
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
        }
        #endregion

        #region Private Methods
        private async Task LoadCustomersAsync()
        {
            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                List<Models.Customer>? response = await _httpClient.GetFromJsonAsync<List<Models.Customer>>(new Uri(new Uri(baseUrl), "customer"));

                Customers.Clear();
                if (response != null)
                {
                    foreach (Models.Customer customer in response)
                        Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
