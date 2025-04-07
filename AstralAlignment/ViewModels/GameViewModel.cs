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
using System.Windows.Media.Imaging;
using System.Diagnostics;

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
            Debug.WriteLine($"Creating GameViewModel with rows: {_game.Rows}, columns: {_game.Columns}");
            Debug.WriteLine($"Total cards in game: {_game.Cards.Count}");

            // Initialize card view models
            Cards = new ObservableCollection<CardViewModel>();

            // Set card dimensions based on grid size
            ScaleCardDimensions();

            // Initialize card view models
            InitializeCardViewModels();

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

                Cards.Add(cardViewModel);
            }

            Debug.WriteLine($"Initialized {Cards.Count} card view models");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_game.IsCompleted)
            {
                _game.ElapsedTime = DateTime.Now - _game.StartTime;
                OnPropertyChanged(nameof(ElapsedTimeDisplay));
            }
        }

        private void OnCardClick(object parameter)
        {
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
                    GameCompleted();
                }
            }

            // Reset for next turn
            _firstCard = null;
            _isProcessingTurn = false;
        }

        private async void ProcessMismatch(CardViewModel secondCard)
        {
            // Wait before flipping back
            await System.Threading.Tasks.Task.Delay(1000);

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

        private void GameCompleted()
        {
            _game.IsCompleted = true;
            _timer.Stop();

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

            // Show completion message
            MessageBox.Show($"Congratulations! You completed the game in {ElapsedTimeDisplay} with {Moves} moves.",
                "Game Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanClickCard(object parameter)
        {
            return parameter is CardViewModel cardViewModel &&
                   !cardViewModel.IsFlipped &&
                   !cardViewModel.IsMatched &&
                   !_game.IsCompleted &&
                   !_isProcessingTurn;
        }

        private void StartNewGame()
        {
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
            }
        }

        private void ReturnToSetup()
        {
            // Navigate back to setup
            var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainVM != null)
            {
                var setupView = new Views.GameSetUpView(_game.Player);
                mainVM.CurrentView = setupView;
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
                    if (value)
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
                }
            }
        }

        public CardViewModel(MemoryCard card)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            _isFlipped = card.IsFlipped;
            _isMatched = card.IsMatched;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}