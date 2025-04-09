using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AstralAlignment.Models;
using AstralAlignment.Services;
using System.Linq;
using System.Windows;

namespace AstralAlignment.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly GameService _gameService;
        private readonly UserDataService _userDataService;

        private ObservableCollection<UserStatisticsViewModel> _userStatistics;
        public ObservableCollection<UserStatisticsViewModel> UserStatistics
        {
            get => _userStatistics;
            set
            {
                _userStatistics = value;
                OnPropertyChanged(nameof(UserStatistics));
            }
        }

        public StatisticsViewModel()
        {
            _gameService = new GameService();
            _userDataService = new UserDataService();
            UserStatistics = new ObservableCollection<UserStatisticsViewModel>();
            LoadStatisticsAsync();
        }

        private async void LoadStatisticsAsync()
        {
            try
            {
                // Load all users
                var users = await _userDataService.LoadUsersAsync();

                // Load statistics for each user
                foreach (var user in users)
                {
                    try
                    {
                        string userStatsFile = Path.Combine("Statistics", $"{user.Name}.json");
                        if (File.Exists(userStatsFile))
                        {
                            using (FileStream fs = new FileStream(userStatsFile, FileMode.Open))
                            {
                                var stats = await JsonSerializer.DeserializeAsync<UserStatistics>(fs);
                                if (stats != null)
                                {
                                    var statsVM = new UserStatisticsViewModel
                                    {
                                        Username = stats.Username,
                                        TotalGames = stats.TotalGames,
                                        GamesWon = stats.GamesWon,
                                        WinRate = stats.TotalGames > 0 ? (double)stats.GamesWon / stats.TotalGames : 0,
                                        ZodiacImagePath = user.ZodiacImagePath
                                    };
                                    UserStatistics.Add(statsVM);
                                }
                            }
                        }
                        else
                        {
                            // User has no statistics yet
                            var statsVM = new UserStatisticsViewModel
                            {
                                Username = user.Name,
                                TotalGames = 0,
                                GamesWon = 0,
                                WinRate = 0,
                                ZodiacImagePath = user.ZodiacImagePath
                            };
                            UserStatistics.Add(statsVM);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other users
                        Console.WriteLine($"Error loading statistics for {user.Name}: {ex.Message}");
                    }
                }

                // Sort by username
                UserStatistics = new ObservableCollection<UserStatisticsViewModel>(
                    UserStatistics.OrderBy(s => s.Username)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}",
                    "Statistics Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ViewModel for each user's statistics row
    public class UserStatisticsViewModel
    {
        public string Username { get; set; }
        public int TotalGames { get; set; }
        public int GamesWon { get; set; }
        public double WinRate { get; set; }
        public string ZodiacImagePath { get; set; }
    }
}