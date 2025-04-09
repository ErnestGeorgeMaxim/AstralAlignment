// Dynamic source for ComboBoxes based on category
using AstralAlignment.Commands;
using AstralAlignment.Models;
using AstralAlignment.Services;
using AstralAlignment.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;


namespace AstralAlignment.ViewModels
{
    public class GameSetUpViewModel : INotifyPropertyChanged
    {
        public IEnumerable<int> SizesSource => IsZodiacCategory ? ZodiacSizes : OtherSizes;
        private readonly User _currentUser;
        private string _selectedCategory;
        private bool _isStandardMode = true;
        private int _rows = 4; // Default value
        private int _columns = 4; // Default value
        private int _timeLimitMinutes = 3; // Default time limit (3 minutes)
        private readonly GameService _gameService;

        // Available categories
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>
        {
            "Zodiac Signs",
            "Celestial Bodies",
            "Constellations"
        };

        // Available row/column sizes based on category
        public ObservableCollection<int> ZodiacSizes { get; } = new ObservableCollection<int> { 2, 3, 4 };
        public ObservableCollection<int> OtherSizes { get; } = new ObservableCollection<int> { 2, 3, 4, 5, 6 };

        // Properties for binding
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;

                    // Adjust rows/columns if needed when switching categories
                    if (_selectedCategory == "Zodiac Signs" && (_rows > 4 || _columns > 4))
                    {
                        Rows = Math.Min(_rows, 4);
                        Columns = Math.Min(_columns, 4);
                    }

                    OnPropertyChanged(nameof(SelectedCategory));
                    OnPropertyChanged(nameof(IsZodiacCategory));
                    OnPropertyChanged(nameof(SizesSource));
                }
            }
        }

        public bool IsZodiacCategory => SelectedCategory == "Zodiac Signs";

        // Available time limits in minutes
        public ObservableCollection<int> TimeOptions { get; } = new ObservableCollection<int> { 1, 2, 3, 5, 10 };

        // Time limit in minutes
        public int TimeLimitMinutes
        {
            get => _timeLimitMinutes;
            set
            {
                _timeLimitMinutes = value;
                OnPropertyChanged(nameof(TimeLimitMinutes));
            }
        }

        public bool IsStandardMode
        {
            get => _isStandardMode;
            set
            {
                _isStandardMode = value;
                OnPropertyChanged(nameof(IsStandardMode));
                OnPropertyChanged(nameof(IsCustomMode));
            }
        }

        public bool IsCustomMode
        {
            get => !_isStandardMode;
            set
            {
                _isStandardMode = !value;
                OnPropertyChanged(nameof(IsStandardMode));
                OnPropertyChanged(nameof(IsCustomMode));
            }
        }

        // Use direct value binding for ComboBox
        public int Rows
        {
            get => _rows;
            set
            {
                // Enforce maximum based on category
                if (IsZodiacCategory && value > 4)
                {
                    _rows = 4;
                }
                else
                {
                    _rows = value;
                }

                OnPropertyChanged(nameof(Rows));
                Debug.WriteLine($"Rows set to {_rows}");
            }
        }

        public int Columns
        {
            get => _columns;
            set
            {
                // Enforce maximum based on category
                if (IsZodiacCategory && value > 4)
                {
                    _columns = 4;
                }
                else
                {
                    _columns = value;
                }

                OnPropertyChanged(nameof(Columns));
                Debug.WriteLine($"Columns set to {_columns}");
            }
        }

        // Commands
        public ICommand NewGameCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand ViewStatisticsCommand { get; }
        public ICommand ExitCommand { get; }

        public GameSetUpViewModel(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _gameService = new GameService();

            // Default category
            SelectedCategory = Categories[0];

            // Initialize commands
            NewGameCommand = new RelayCommand(_ => StartNewGame(), _ => CanStartNewGame());
            OpenGameCommand = new RelayCommand(_ => OpenGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame(), _ => false); // Disabled initially
            ViewStatisticsCommand = new RelayCommand(_ => ShowStatistics());
            ExitCommand = new RelayCommand(_ => NavigateToProfileView());
        }

        private void StartNewGame()
        {
            try
            {
                // Get board size
                int rows = IsStandardMode ? 4 : Rows;
                int columns = IsStandardMode ? 4 : Columns;

                // Additional validation for Zodiac category
                if (SelectedCategory == "Zodiac Signs")
                {
                    // Enforce maximum of 4 rows and columns for Zodiac
                    if (rows > 4)
                    {
                        rows = 4;
                        Debug.WriteLine("Rows limited to 4 for Zodiac category");
                    }
                    if (columns > 4)
                    {
                        columns = 4;
                        Debug.WriteLine("Columns limited to 4 for Zodiac category");
                    }
                }

                // Debug information
                Debug.WriteLine($"Starting new game with mode: {(IsStandardMode ? "Standard" : "Custom")}");
                Debug.WriteLine($"Board dimensions: {rows}x{columns}");
                Debug.WriteLine($"Selected category: {SelectedCategory}");

                // Check if number of cards is even
                if ((rows * columns) % 2 != 0)
                {
                    MessageBox.Show("The board must have an even number of cards. Please adjust the dimensions.",
                        "Invalid Board Size", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new game with time limit
                var game = new MemoryGame(_currentUser, SelectedCategory, rows, columns, TimeSpan.FromMinutes(TimeLimitMinutes));
                Debug.WriteLine($"Game created with {game.Cards.Count} cards and time limit: {TimeLimitMinutes} minutes");

                // Navigate to game view
                var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
                if (mainVM != null)
                {
                    var gameView = new Views.GameView(game);
                    mainVM.CurrentView = gameView;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating game: {ex.Message}");
                MessageBox.Show($"Error starting game: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanStartNewGame()
        {
            // Check if category is selected
            return !string.IsNullOrEmpty(SelectedCategory);
        }

        private void OpenGame()
        {
            // Show open file dialog to select a saved game
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json",
                Title = "Open Saved Game"
            };

            if (dialog.ShowDialog() == true)
            {
                LoadGameAsync(dialog.FileName);
            }
        }

        private async void LoadGameAsync(string filePath)
        {
            try
            {
                var loadedGame = await _gameService.LoadGameAsync(filePath, _currentUser);
                if (loadedGame != null)
                {
                    // Navigate to game view with loaded game
                    var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
                    if (mainVM != null)
                    {
                        var gameView = new Views.GameView(loadedGame);
                        mainVM.CurrentView = gameView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading game: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGame()
        {
            // This function is called from GameView, not directly from setup
            MessageBox.Show("You can save the game while playing.", "Information",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowStatistics()
        {
            try
            {
                // Create the statistics view model
                var statisticsViewModel = new StatisticsViewModel();

                // Get the main window reference
                var mainWindow = Application.Current.MainWindow as AstralAlignment.View.MainWindow;
                if (mainWindow != null)
                {
                    // Show the statistics overlay on the main window
                    mainWindow.ShowStatisticsOverlay(statisticsViewModel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing statistics: {ex.Message}", "Statistics Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToProfileView()
        {
            // Return to profile view
            var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainVM != null)
            {
                mainVM.ShowProfileViewCommand.Execute(null);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}