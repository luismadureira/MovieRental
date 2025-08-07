using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MovieRental.WPF.ViewModels
{
    public class MoviesViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _disposed = false;
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
                using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "movie"), newMovie)
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Movie added!";
                    ClearForm();
                    await LoadMoviesAsync();
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
