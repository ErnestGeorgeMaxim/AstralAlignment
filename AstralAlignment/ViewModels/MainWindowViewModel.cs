using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AstralAlignment.Commands;
using AstralAlignment.Views;

namespace AstralAlignment.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public RelayCommand ShowStartUpViewCommand { get; }
        public RelayCommand ShowProfileViewCommand { get; }

        public MainWindowViewModel()
        {
            ShowStartUpViewCommand = new RelayCommand(_ => CurrentView = new StartUpView());
            ShowProfileViewCommand = new RelayCommand(_ => CurrentView = new ProfileView());
            CurrentView = new StartUpView();

            // Register commands as application resources
            Application.Current.Resources["ShowStartUpViewCommand"] = ShowStartUpViewCommand;
            Application.Current.Resources["ShowProfileViewCommand"] = ShowProfileViewCommand;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}