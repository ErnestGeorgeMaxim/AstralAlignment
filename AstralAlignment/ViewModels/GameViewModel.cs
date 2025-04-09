using AstralAlignment.Commands;
using AstralAlignment.Models;
using AstralAlignment.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using AstralAlignment.Views;

namespace AstralAlignment.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly MemoryGame _game;
        private readonly DispatcherTimer _timer;
        private MemoryCard _firstCard;
        private bool _isProcessingTurn;
        private readonly GameService _gameService;

        // Card dimension properties
        private double _cardWidth = 120;
        private double _cardHeight = 174;

        // New property for time display
        private string _timeDisplay;
        public string TimeDisplay
        {
            get => _timeDisplay;
            set
            {
                _timeDisplay = value;
                OnPropertyChanged(nameof(TimeDisplay));
            }
        }

        // New property for time color
        private bool _isTimeAlmostExpired;
        public bool IsTimeAlmostExpired
        {
            get => _isTimeAlmostExpired;
            set
            {
                _isTimeAlmostExpired = value;
                OnPropertyChanged(nameof(IsTimeAlmostExpired));
            }
        }

        // Properties for card dimensions
        public double CardWidth
        {
            get => _cardWidth;
            set
            {
                _cardWidth = value;
                OnPropertyChanged(nameof(CardWidth));
            }
        }

        public double CardHeight
        {
            get => _cardHeight;
            set
            {
                _cardHeight = value;
                OnPropertyChanged(nameof(CardHeight));
            }
        }

        // Game reference
        public MemoryGame Game => _game;

        // Observable card collection
        public ObservableCollection<CardViewModel> Cards { get; }

        // Game state properties
        public int Moves => _game.Moves;
        public string MatchesDisplay => $"{_game.MatchesFound}/{_game.Cards.Count / 2}";
        public string ElapsedTimeDisplay => $"{_game.ElapsedTime.Minutes:D2}:{_game.ElapsedTime.Seconds:D2}";

        // Time limit for display
        public string TimeLimitDisplay => $"{(int)_game.TimeLimit.TotalMinutes:D2}:{_game.TimeLimit.Seconds:D2}";

        // Commands
        public ICommand CardClickCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand ReturnToSetupCommand { get; }

        public GameViewModel(MemoryGame game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _gameService = new GameService();

            // Debug information about game settings
            Debug.WriteLine($"Creating GameViewModel with rows: {_game.Rows}, columns: {_game.Columns}, time limit: {_game.TimeLimit.TotalMinutes} minutes");
            Debug.WriteLine($"Total cards in game: {_game.Cards.Count}");

            // Initialize card view models
            Cards = new ObservableCollection<CardViewModel>();

            // Set card dimensions based on grid size
            ScaleCardDimensions();

            // Initialize card view models
            InitializeCardViewModels();

            // Initialize timer display
            UpdateTimeDisplay();

            // Initialize timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Initialize commands
            CardClickCommand = new RelayCommand(OnCardClick, CanClickCard);
            NewGameCommand = new RelayCommand(_ => StartNewGame());
            SaveGameCommand = new RelayCommand(_ => SaveGame());
            ReturnToSetupCommand = new RelayCommand(_ => ReturnToSetup());
        }

        private void ScaleCardDimensions()
        {
            try
            {
                // Base dimensions for card
                double baseWidth = 120;
                double baseHeight = 174;

                // Scale based on grid size
                int maxDimension = Math.Max(_game.Rows, _game.Columns);
                double scaleFactor;

                // Adjust scale based on grid size
                if (maxDimension >= 6) scaleFactor = 2.5;
                else if (maxDimension >= 5) scaleFactor = 2.0;
                else if (maxDimension >= 4) scaleFactor = 1.5;
                else scaleFactor = 1.0;

                // Apply scaling
                CardWidth = baseWidth / scaleFactor;
                CardHeight = baseHeight / scaleFactor;

                Debug.WriteLine($"Scaled card dimensions: {CardWidth}x{CardHeight} for grid size {_game.Rows}x{_game.Columns}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scaling card dimensions: {ex.Message}");
                // Fallback to default dimensions
                CardWidth = 120;
                CardHeight = 174;
            }
        }

        private void InitializeCardViewModels()
        {
            Cards.Clear();
            Debug.WriteLine($"Initializing {_game.Cards.Count} card view models");

            foreach (var card in _game.Cards)
            {
                var cardViewModel = new CardViewModel(card);

                // Construct image path based on category and value
                string folderName;
                // Make sure filename is correctly formatted - lowercase with no spaces
                string fileName = card.Value.ToLower().Replace(" ", "-");

                // Map category names to folder names
                switch (_game.Category)
                {
                    case "Celestial Bodies":
                        folderName = "planets"; // Use planets folder for Celestial Bodies category
                        break;
                    case "Zodiac Signs":
                        folderName = "zodiac";
                        break;
                    case "Constellations":
                        folderName = "constellations";
                        break;
                    default:
                        folderName = _game.Category.ToLower().Replace(" ", "-");
                        break;
                }

                string imagePath = $"/Images/{folderName}/{fileName}.png";
                cardViewModel.ImageSource = imagePath;

                // Ensure card state is properly reflected in view model
                cardViewModel.IsFlipped = card.IsFlipped;
                cardViewModel.IsMatched = card.IsMatched;

                Cards.Add(cardViewModel);
            }

            Debug.WriteLine($"Initialized {Cards.Count} card view models");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_game.IsCompleted)
            {
                // Update elapsed time
                _game.ElapsedTime = DateTime.Now - _game.StartTime;
                OnPropertyChanged(nameof(ElapsedTimeDisplay));

                // Update time display with remaining time
                UpdateTimeDisplay();

                // Check if time expired
                if (_game.IsTimeExpired && !_game.IsCompleted)
                {
                    GameLost();
                }
            }
        }

        private void UpdateTimeDisplay()
        {
            // Calculate remaining time
            TimeSpan remaining = _game.RemainingTime;

            // Format display with minutes and seconds
            TimeDisplay = $"{Math.Max(0, (int)remaining.TotalMinutes):D2}:{Math.Max(0, remaining.Seconds):D2}";

            // Set warning flag if less than 20% of time remains
            IsTimeAlmostExpired = remaining.TotalSeconds < (_game.TimeLimit.TotalSeconds * 0.2);
        }

        private void OnCardClick(object parameter)
        {
            // Don't allow card flips if time has expired
            if (_game.IsTimeExpired)
                return;

            if (_isProcessingTurn)
                return;

            if (parameter is CardViewModel cardViewModel && !cardViewModel.IsFlipped && !cardViewModel.IsMatched)
            {
                // Flip the card
                cardViewModel.IsFlipped = true;

                if (_firstCard == null)
                {
                    // First card of the pair
                    _firstCard = cardViewModel.Card;
                }
                else
                {
                    // Second card - process the turn
                    _isProcessingTurn = true;
                    _game.Moves++;
                    OnPropertyChanged(nameof(Moves));

                    // Check for match
                    if (_firstCard.MatchesWith(cardViewModel.Card))
                    {
                        // Match found
                        ProcessMatch(cardViewModel);
                    }
                    else
                    {
                        // No match - flip back after delay
                        ProcessMismatch(cardViewModel);
                    }
                }
            }
        }

        private void ProcessMatch(CardViewModel secondCard)
        {
            // Set both cards as matched
            var firstCardViewModel = Cards.FirstOrDefault(c => c.Card == _firstCard);
            if (firstCardViewModel != null)
            {
                firstCardViewModel.IsMatched = true;
                secondCard.IsMatched = true;

                _firstCard.SetMatched();
                secondCard.Card.SetMatched();

                _game.MatchesFound++;
                OnPropertyChanged(nameof(MatchesDisplay));

                // Check if game is completed
                if (_game.MatchesFound >= _game.Cards.Count / 2)
                {
                    GameWon();
                }
            }

            // Reset for next turn
            _firstCard = null;
            _isProcessingTurn = false;
        }

        private void CleanupResources()
        {
            // Stop the timer if it's running
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
                Debug.WriteLine("Timer stopped during cleanup");
            }
        }

        private async void ProcessMismatch(CardViewModel secondCard)
        {
            // Wait before flipping back
            await System.Threading.Tasks.Task.Delay(400);

            // Flip both cards back
            var firstCardViewModel = Cards.FirstOrDefault(c => c.Card == _firstCard);
            if (firstCardViewModel != null)
            {
                firstCardViewModel.IsFlipped = false;
            }
            secondCard.IsFlipped = false;

            // Reset for next turn
            _firstCard = null;
            _isProcessingTurn = false;
        }

        private void GameWon()
        {
            _game.IsCompleted = true;
            CleanupResources(); // Make sure to stop the timer

            // Create game result for statistics
            var result = new GameResult
            {
                Category = _game.Category,
                Rows = _game.Rows,
                Columns = _game.Columns,
                Moves = _game.Moves,
                Duration = _game.ElapsedTime,
                IsWon = true,
                Date = DateTime.Now
            };

            // Update statistics asynchronously
            _ = _gameService.UpdateStatisticsAsync(_game.Player, result);

            // Show game result dialog instead of MessageBox
            ShowGameResultDialog(true);
        }

        private void GameLost()
        {
            _game.IsCompleted = true;
            CleanupResources(); // Make sure to stop the timer

            // Create game result for statistics - loss
            var result = new GameResult
            {
                Category = _game.Category,
                Rows = _game.Rows,
                Columns = _game.Columns,
                Moves = _game.Moves,
                Duration = _game.ElapsedTime,
                IsWon = false,
                Date = DateTime.Now
            };

            // Update statistics asynchronously
            _ = _gameService.UpdateStatisticsAsync(_game.Player, result);

            // Show game result dialog instead of MessageBox
            ShowGameResultDialog(false);
        }

        private void ShowGameResultDialog(bool isWon)
        {
            try
            {
                // Get the main window view model
                var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
                if (mainVM != null)
                {
                    // Create the game result dialog with current game stats
                    // Enable auto-redirect by default
                    var resultDialog = new GameResultDialog(
                        isWon,
                        ElapsedTimeDisplay,
                        Moves,
                        MatchesDisplay
                    );

                    // Subscribe to action selected event
                    resultDialog.ActionSelected += (sender, action) =>
                    {
                        // Clear the dialog from view
                        mainVM.DialogContent = null;

                        if (action == GameResultDialog.GameResultAction.NewGame)
                        {
                            StartNewGame();
                        }
                        else
                        {
                            ReturnToSetup();
                        }
                    };

                    // Show the dialog by adding it to the main window's content
                    mainVM.DialogContent = resultDialog;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing game result dialog: {ex.Message}");

                // Fallback to message box if dialog fails
                string message = isWon
                    ? $"Congratulations! You completed the game in {ElapsedTimeDisplay} with {Moves} moves."
                    : $"Time's up! You found {_game.MatchesFound} out of {_game.Cards.Count / 2} matches.";

                MessageBox.Show(message,
                    isWon ? "Game Completed" : "Game Over",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Return to game setup view as fallback
                ReturnToSetup();
            }
        }


        private bool CanClickCard(object parameter)
        {
            return parameter is CardViewModel cardViewModel &&
                   !cardViewModel.IsFlipped &&
                   !cardViewModel.IsMatched &&
                   !_game.IsCompleted &&
                   !_isProcessingTurn &&
                   !_game.IsTimeExpired;
        }

        private void StartNewGame()
        {
            // Clean up resources before starting a new game
            CleanupResources();

            // Navigate back to setup and start new game with same user
            var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainVM != null)
            {
                var setupView = new Views.GameSetUpView(_game.Player);
                mainVM.CurrentView = setupView;
            }
        }

        private async void SaveGame()
        {
            // Show save dialog
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json",
                Title = "Save Game"
            };

            if (dialog.ShowDialog() == true)
            {
                await _gameService.SaveGameAsync(_game, dialog.FileName);
                MessageBox.Show("Game saved successfully!", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReturnToSetup()
        {
            try
            {
                // Clean up resources before navigating away
                CleanupResources();

                // Navigate back to setup with the current user
                var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
                if (mainVM != null)
                {
                    var setupView = new Views.GameSetUpView(_game.Player);
                    mainVM.CurrentView = setupView;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error returning to GameSetUpView: {ex.Message}");
                MessageBox.Show($"Error returning to game setup: {ex.Message}",
                    "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class CardViewModel : INotifyPropertyChanged
    {
        private bool _isFlipped;
        private bool _isMatched;
        private string _imageSource;

        public MemoryCard Card { get; }

        public string ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                if (_isFlipped != value)
                {
                    _isFlipped = value;
                    OnPropertyChanged(nameof(IsFlipped));

                    // Make sure the underlying card model is updated
                    if (value != Card.IsFlipped)
                    {
                        Card.Flip();
                    }
                }
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                if (_isMatched != value)
                {
                    _isMatched = value;
                    OnPropertyChanged(nameof(IsMatched));

                    // Make sure the underlying card model is updated
                    if (value && !Card.IsMatched)
                    {
                        Card.SetMatched();
                    }
                }
            }
        }

        public CardViewModel(MemoryCard card)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));

            // Initialize view model state from the card model
            _isFlipped = card.IsFlipped;
            _isMatched = card.IsMatched;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}