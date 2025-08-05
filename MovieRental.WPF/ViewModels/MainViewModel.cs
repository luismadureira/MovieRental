using MovieRental.WPF.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace MovieRental.WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private UserControl _currentView;

        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public RelayCommand OpenCustomersViewCommand { get; }
        public RelayCommand OpenMoviesViewCommand { get; }
        public RelayCommand OpenRentalsViewCommand { get; }

        public MainViewModel()
        {
            OpenCustomersViewCommand = new RelayCommand(OpenCustomersView);
            OpenMoviesViewCommand = new RelayCommand(OpenMoviesView);
            OpenRentalsViewCommand = new RelayCommand(OpenRentalsView);
        }

        private void OpenRentalsView()
        {
            CurrentView = new RentalsView();
        }

        private void OpenMoviesView()
        {
            CurrentView = new MoviesView();
        }

        private void OpenCustomersView()
        {
            CurrentView = new CustomersView();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
