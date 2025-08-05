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
        private readonly HttpClient _httpClient = new HttpClient();

        public ObservableCollection<Models.Movie> Movies { get; set; } = new();

        public string Title { get; set; }
        public string StatusMessage { get; set; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public MoviesViewModel()
        {
            SaveCommand = new RelayCommand(async () => await SaveMovieAsync());
            _ = LoadMoviesAsync();
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading movies: {ex.Message}";
            }
        }

        public async Task SaveMovieAsync()
        {
            try
            {
                Models.Movie newMovie = new Models.Movie { Title = Title };
                string? baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                HttpResponseMessage result = await _httpClient.PostAsJsonAsync(new Uri(new Uri(baseUrl), "movie"), newMovie);

                if (result.IsSuccessStatusCode)
                {
                    StatusMessage = "Movie added!";
                    Title = string.Empty;
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

            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(Title));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
