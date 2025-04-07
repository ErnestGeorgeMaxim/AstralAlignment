using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AstralAlignment.Models;
using AstralAlignment.Services;
using AstralAlignment.Commands;

namespace AstralAlignment.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly UserDataService _userDataService;

        // Properties for the zodiac signs
        public ObservableCollection<ZodiacSign> ZodiacSigns { get; } = new ObservableCollection<ZodiacSign>();

        // Collection of users
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        // Selected user
        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    IsInCreationMode = _selectedUser == null;
                    OnPropertyChanged(nameof(SelectedUser));
                    OnPropertyChanged(nameof(CanDeleteUser));

                    // Update current zodiac when user is selected
                    if (_selectedUser != null)
                    {
                        var zodiacSign = ZodiacSigns.FirstOrDefault(z => z.Name == _selectedUser.ZodiacSignName);
                        if (zodiacSign != null)
                        {
                            CurrentZodiacSign = zodiacSign;
                        }
                    }
                }
            }
        }

        // Current zodiac sign (for selection or display)
        private ZodiacSign _currentZodiacSign;
        public ZodiacSign CurrentZodiacSign
        {
            get => _currentZodiacSign;
            set
            {
                if (_currentZodiacSign != value)
                {
                    _currentZodiacSign = value;
                    OnPropertyChanged(nameof(CurrentZodiacSign));
                }
            }
        }

        // User name input
        private string _userNameInput;
        public string UserNameInput
        {
            get => _userNameInput;
            set
            {
                if (_userNameInput != value)
                {
                    _userNameInput = value;
                    OnPropertyChanged(nameof(UserNameInput));
                    OnPropertyChanged(nameof(CanCreateUser));
                    // Add this to notify command availability when text changes
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // Creation mode flag
        private bool _isInCreationMode = true;
        public bool IsInCreationMode
        {
            get => _isInCreationMode;
            private set
            {
                if (_isInCreationMode != value)
                {
                    _isInCreationMode = value;
                    OnPropertyChanged(nameof(IsInCreationMode));
                    OnPropertyChanged(nameof(CreateButtonText));
                    // Add this to notify command availability when mode changes
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // Button text changes based on mode
        public string CreateButtonText => IsInCreationMode ? "CREATE USER" : "NEW USER";

        // Command availability
        public bool CanCreateUser => IsInCreationMode && !string.IsNullOrWhiteSpace(UserNameInput);
        public bool CanDeleteUser => !IsInCreationMode && SelectedUser != null;


        public ICommand PlayGameCommand { get; }
        public ICommand NextZodiacCommand { get; }
        public ICommand PrevZodiacCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public ICommand CancelSelectionCommand { get; }

        public ProfileViewModel(UserDataService userDataService)
        {
            _userDataService = userDataService ?? throw new ArgumentNullException(nameof(userDataService));

            // Initialize commands
            NextZodiacCommand = new RelayCommand(_ => NextZodiac());
            PrevZodiacCommand = new RelayCommand(_ => PrevZodiac());
            CreateUserCommand = new RelayCommand(_ => CreateUser(), _ => CanCreateUser);
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => CanDeleteUser);

            // Initialize the PlayGameCommand
            PlayGameCommand = new RelayCommand(_ => PlayGame(), _ => CanPlayGame);

            // Add this new command for canceling
            CancelSelectionCommand = new RelayCommand(_ => CancelSelection());

            // Initialize zodiac signs
            InitializeZodiacSigns();

            // Set default current zodiac sign
            CurrentZodiacSign = ZodiacSigns.FirstOrDefault(z => z.Name == "Leo") ?? ZodiacSigns.FirstOrDefault();

            // Load users
            LoadUsersAsync();
        }

        // Add a CanPlayGame property
        public bool CanPlayGame => SelectedUser != null;

        private void PlayGame()
        {
            try
            {
                if (SelectedUser != null)
                {
                    // Get the MainWindowViewModel
                    var mainWindow = Application.Current.MainWindow;
                    var mainVM = mainWindow?.DataContext as MainWindowViewModel;

                    if (mainVM != null)
                    {
                        // Create a new GameSetUpView
                        var gameSetUpView = new Views.GameSetUpView(SelectedUser);

                        // Set it as the current view
                        mainVM.CurrentView = gameSetUpView;
                    }
                    else
                    {
                        MessageBox.Show("Could not access MainWindowViewModel.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting game: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this method to the ProfileViewModel class
        private void CancelSelection()
        {
            // Clear the current selection
            SelectedUser = null;

            // Reset the user name input field
            UserNameInput = string.Empty;

            // Set to creation mode
            IsInCreationMode = true;

            // Reset to default zodiac if needed
            CurrentZodiacSign = ZodiacSigns.FirstOrDefault(z => z.Name == "Leo") ?? ZodiacSigns.FirstOrDefault();
        }

        private void InitializeZodiacSigns()
        {
            ZodiacSigns.Add(new ZodiacSign { Name = "Aries", ImagePath = "/Images/icons/aries.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Taurus", ImagePath = "/Images/icons/taurus.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Gemini", ImagePath = "/Images/icons/gemini.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Cancer", ImagePath = "/Images/icons/cancer.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Leo", ImagePath = "/Images/icons/leo.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Virgo", ImagePath = "/Images/icons/virgo.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Libra", ImagePath = "/Images/icons/libra.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Scorpio", ImagePath = "/Images/icons/scorpio.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Sagittarius", ImagePath = "/Images/icons/sagittarius.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Capricorn", ImagePath = "/Images/icons/capricorn.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Aquarius", ImagePath = "/Images/icons/aquarius.png" });
            ZodiacSigns.Add(new ZodiacSign { Name = "Pisces", ImagePath = "/Images/icons/pisces.png" });
        }

        private async void LoadUsersAsync()
        {
            try
            {
                var users = await _userDataService.LoadUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                // Select first user if any exist
                if (Users.Count > 0)
                {
                    SelectedUser = Users[0];
                }
                else
                {
                    // Ensure we're in creation mode if no users exist
                    IsInCreationMode = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveUsersAsync()
        {
            try
            {
                await _userDataService.SaveUsersAsync(Users.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextZodiac()
        {
            int currentIndex = ZodiacSigns.IndexOf(CurrentZodiacSign);
            int nextIndex = (currentIndex + 1) % ZodiacSigns.Count;
            CurrentZodiacSign = ZodiacSigns[nextIndex];
        }

        private void PrevZodiac()
        {
            int currentIndex = ZodiacSigns.IndexOf(CurrentZodiacSign);
            int prevIndex = (currentIndex - 1 + ZodiacSigns.Count) % ZodiacSigns.Count;
            CurrentZodiacSign = ZodiacSigns[prevIndex];
        }

        private async void CreateUser()
        {
            if (IsInCreationMode)
            {
                // Create a new user
                var newUser = new User
                {
                    Name = UserNameInput,
                    ZodiacSignName = CurrentZodiacSign.Name,
                    ZodiacImagePath = CurrentZodiacSign.ImagePath
                };

                Users.Add(newUser);
                await SaveUsersAsync();

                // Select the new user
                SelectedUser = newUser;
                UserNameInput = string.Empty;
            }
            else
            {
                // Switch to creation mode
                SelectedUser = null;
                UserNameInput = string.Empty;
                IsInCreationMode = true;
            }
        }

        private async void DeleteUser()
        {
            if (SelectedUser != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {SelectedUser.Name}?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Users.Remove(SelectedUser);
                    await SaveUsersAsync();
                    SelectedUser = Users.FirstOrDefault();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ZodiacSign class for the available zodiac signs
    public class ZodiacSign
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
    }
}