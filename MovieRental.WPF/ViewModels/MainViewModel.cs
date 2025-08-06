using MovieRental.WPF.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace MovieRental.WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields
        private UserControl? _currentView;
        #endregion

        #region Properties
        public UserControl? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public RelayCommand OpenCustomersViewCommand { get; }
        public RelayCommand OpenMoviesViewCommand { get; }
        public RelayCommand OpenRentalsViewCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            OpenCustomersViewCommand = new RelayCommand(OpenCustomersView);
            OpenMoviesViewCommand = new RelayCommand(OpenMoviesView);
            OpenRentalsViewCommand = new RelayCommand(OpenRentalsView);
        }
        #endregion

        #region Command Methods
        private void OpenCustomersView()
        {
            CurrentView = new CustomersView();
        }

        private void OpenMoviesView()
        {
            CurrentView = new MoviesView();
        }

        private void OpenRentalsView()
        {
            CurrentView = new RentalsView();
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
