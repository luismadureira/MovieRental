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
    public class MoviesViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private bool _disposed = false;
        private string _title = string.Empty;
        private string _statusMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private string _searchTitle = string.Empty;
        private bool _isLoading = false;
        private Models.Movie? _selectedMovie;
        #endregion

        #region Properties
        public ObservableCollection<Models.Movie> Movies { get; set; } = new();

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); ValidateForm(); }
        }

        public string SearchTitle
        {
            get => _searchTitle;
            set { _searchTitle = value; OnPropertyChanged(); }
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

        public Models.Movie? SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                _selectedMovie = value;
                OnPropertyChanged();
                LoadMovieForEdit();
            }
        }

        public bool IsFormValid => !string.IsNullOrWhiteSpace(Title);
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        #endregion

        #region Constructor
        public MoviesViewModel() : this(CreateHttpClient())
        {
        }

        public MoviesViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            SaveCommand = new RelayCommand(async () => await SaveMovieAsync().ConfigureAwait(false), () => IsFormValid && !IsLoading);

            _ = Task.Run(async () => await LoadMoviesAsync());
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            return client;
        }
        #endregion

        #region Public Methods
        public async Task SaveMovieAsync()
        {
            if (!IsFormValid) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                Models.Movie movie = new Models.Movie
                {
                    Id = SelectedMovie?.Id ?? 0,
                    Title = Title.Trim()
                };

                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                HttpResponseMessage result;

                if (movie.Id == 0)
                {
                    result = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "movie"), movie)
                        .ConfigureAwait(false);
                }
                else
                {
                    result = await _httpClient.PutAsJsonAsync(new Uri(new Uri(baseUrl), $"movie/{movie.Id}"), movie)
                        .ConfigureAwait(false);
                }

                if (result.IsSuccessStatusCode)
                {
                    StatusMessage = movie.Id == 0 ? "Movie added!" : "Movie updated!";
                    ClearForm();
                    await LoadMoviesAsync().ConfigureAwait(false);
                }
                else
                {
                    string errorContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ErrorMessage = $"Save failed: {result.StatusCode}. {errorContent}";
                }
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
        private async Task LoadMoviesAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                using HttpResponseMessage response = await _httpClient.GetAsync(new Uri(new Uri(baseUrl), "movie"));

                if (response.IsSuccessStatusCode)
                {
                    List<Models.Movie>? movies = await response.Content.ReadFromJsonAsync<List<Models.Movie>>();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Movies.Clear();
                        if (movies != null)
                        {
                            foreach (Models.Movie? movie in movies.OrderBy(m => m.Title))
                                Movies.Add(movie);
                        }
                    });
                }
                else
                {
                    ErrorMessage = $"Failed to load movies: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading movies: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadMovieForEdit()
        {
            if (SelectedMovie != null)
            {
                Title = SelectedMovie.Title;
            }
        }

        private void ClearForm()
        {
            Title = string.Empty;
            SearchTitle = string.Empty;
            SelectedMovie = null;
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
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
        #endregion
    }
}
