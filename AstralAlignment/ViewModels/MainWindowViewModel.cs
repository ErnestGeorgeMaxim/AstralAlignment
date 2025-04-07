using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AstralAlignment.Commands;
using AstralAlignment.Models;
using AstralAlignment.Views;

namespace AstralAlignment.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand ShowStartUpViewCommand { get; }
        public ICommand ShowProfileViewCommand { get; }
        public ICommand ShowGameSetUpViewCommand { get; }

        public MainWindowViewModel()
        {
            ShowStartUpViewCommand = new RelayCommand(_ => CurrentView = new StartUpView());

            ShowProfileViewCommand = new RelayCommand(_ => CurrentView = new ProfileView());

            ShowGameSetUpViewCommand = new RelayCommand(param => {
                try
                {
                    if (param is User user)
                    {
                        // Create a new GameSetUpView with the user parameter
                        CurrentView = new GameSetUpView(user);
                    }
                    else
                    {
                        // No user provided, show message and redirect to profile
                        MessageBox.Show("Please select a user profile first.", "User Required",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CurrentView = new ProfileView();
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error navigating to Game Setup: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // Set initial view
            CurrentView = new StartUpView();

            // Register commands as application resources
            Application.Current.Resources["ShowStartUpViewCommand"] = ShowStartUpViewCommand;
            Application.Current.Resources["ShowProfileViewCommand"] = ShowProfileViewCommand;
            Application.Current.Resources["ShowGameSetUpViewCommand"] = ShowGameSetUpViewCommand;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}