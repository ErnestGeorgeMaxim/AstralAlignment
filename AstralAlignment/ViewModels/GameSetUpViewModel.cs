using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using AstralAlignment.Commands;
using AstralAlignment.Models;
using AstralAlignment.Services;

namespace AstralAlignment.ViewModels
{
    public class GameSetUpViewModel : INotifyPropertyChanged
    {
        private readonly User _currentUser;
        private string _selectedCategory;
        private bool _isStandardMode = true;
        private int _rowsIndex = 2; // Default to index 2 (value 4)
        private int _columnsIndex = 2; // Default to index 2 (value 4)
        private readonly GameService _gameService;

        // Available categories
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>
        {
            "Zodiac Signs",
            "Celestial Bodies",
            "Constellations"
        };

        // Properties for binding
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
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

        // Use index-based binding for ComboBox
        public int RowsIndex
        {
            get => _rowsIndex;
            set
            {
                _rowsIndex = value;
                OnPropertyChanged(nameof(RowsIndex));
                Debug.WriteLine($"Rows index set to {value}, which is value {IndexToValue(value)}");
            }
        }

        public int ColumnsIndex
        {
            get => _columnsIndex;
            set
            {
                _columnsIndex = value;
                OnPropertyChanged(nameof(ColumnsIndex));
                Debug.WriteLine($"Columns index set to {value}, which is value {IndexToValue(value)}");
            }
        }

        // Calculate actual dimension based on index
        private int IndexToValue(int index)
        {
            return index + 2; // Index 0 = 2, Index 1 = 3, etc.
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
            ExitCommand = new RelayCommand(_ => NavigateToProfileView());
        }

        private void StartNewGame()
        {
            try
            {
                // Calculate board size
                int rows = IsStandardMode ? 4 : IndexToValue(RowsIndex);
                int columns = IsStandardMode ? 4 : IndexToValue(ColumnsIndex);

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

                // Create new game
                var game = new MemoryGame(_currentUser, SelectedCategory, rows, columns);
                Debug.WriteLine($"Game created with {game.Cards.Count} cards");

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
            // Implement open game dialog
            // When a saved game is loaded, navigate to GameView with the loaded game
        }

        private void SaveGame()
        {
            // Implement save game functionality
            // This will be called from GameView, not directly from setup
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