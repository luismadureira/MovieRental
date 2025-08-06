using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MovieRental.WPF.ViewModels
{
    public class MoviesViewModel : INotifyPropertyChanged
    {
        #region Fields
        private readonly HttpClient _httpClient = new HttpClient();
        private string _title = string.Empty;
        private string _statusMessage = string.Empty;
        private string _errorMessage = string.Empty;
        #endregion

        #region Properties
        public ObservableCollection<Models.Movie> Movies { get; set; } = new();

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
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
        public MoviesViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveMovieAsync());
            _ = LoadMoviesAsync();
        }
        #endregion

        #region Public Methods
        public async Task SaveMovieAsync()
        {
            try
            {
                if (Title == string.Empty)
                {
                    StatusMessage = "Title field is required.";
                    return;
                }

                Models.Movie newMovie = new Models.Movie { Title = Title };
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                HttpResponseMessage result = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "movie"), newMovie);

                if (result.IsSuccessStatusCode)
                {
                    StatusMessage = "Movie added!";
                    ClearForm();
                    await LoadMoviesAsync();
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
        private async Task LoadMoviesAsync()
        {
            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                List<Models.Movie>? response = await _httpClient.GetFromJsonAsync<List<Models.Movie>>(new Uri(new Uri(baseUrl), "movie"));

                Movies.Clear();
                if (response != null)
                {
                    foreach (Models.Movie movie in response)
                        Movies.Add(movie);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading movies: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            Title = string.Empty;
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
