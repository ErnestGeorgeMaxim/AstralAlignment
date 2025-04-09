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

        private object _dialogContent;
        public object DialogContent
        {
            get => _dialogContent;
            set
            {
                _dialogContent = value;
                OnPropertyChanged(nameof(DialogContent));
                OnPropertyChanged(nameof(HasDialogContent));
            }
        }

        public bool HasDialogContent => DialogContent != null;

        // Method to clear dialog content
        public void ClearDialog()
        {
            DialogContent = null;
        }

        public ICommand ShowStartUpViewCommand { get; }
        public ICommand ShowProfileViewCommand { get; }
        public ICommand ShowGameSetUpViewCommand { get; }
        public ICommand QuitCommand { get; } // New command for quitting the application

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

            // Add QuitCommand for exiting the application
            QuitCommand = new RelayCommand(_ => {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to quit?",
                    "Quit Application",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            });

            // Set initial view
            CurrentView = new StartUpView();

            // Register commands as application resources
            Application.Current.Resources["ShowStartUpViewCommand"] = ShowStartUpViewCommand;
            Application.Current.Resources["ShowProfileViewCommand"] = ShowProfileViewCommand;
            Application.Current.Resources["ShowGameSetUpViewCommand"] = ShowGameSetUpViewCommand;
            Application.Current.Resources["QuitCommand"] = QuitCommand; // Register quit command
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}