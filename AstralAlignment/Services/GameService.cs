using AstralAlignment.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstralAlignment.Services
{
    public class GameService
    {
        private const string StatisticsDirectoryName = "Statistics";

        public GameService()
        {
            // Ensure statistics directory exists
            if (!Directory.Exists(StatisticsDirectoryName))
            {
                Directory.CreateDirectory(StatisticsDirectoryName);
            }
        }

        public async Task SaveGameAsync(MemoryGame game, string filePath)
        {
            try
            {
                // Create a serializable game state
                var gameState = new GameState
                {
                    PlayerName = game.Player.Name,
                    Category = game.Category,
                    Rows = game.Rows,
                    Columns = game.Columns,
                    Moves = game.Moves,
                    MatchesFound = game.MatchesFound,
                    StartTime = game.StartTime,
                    ElapsedTime = game.ElapsedTime,
                    IsCompleted = game.IsCompleted,
                    Cards = game.Cards
                };

                // Serialize and save
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync(fs, gameState);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving game: {ex.Message}",
                    "Save Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public async Task<MemoryGame> LoadGameAsync(string filePath, User currentUser)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    var gameState = await JsonSerializer.DeserializeAsync<GameState>(fs);

                    // Create a new game with the loaded state
                    var game = new MemoryGame(currentUser, gameState.Category, gameState.Rows, gameState.Columns);

                    // Update game properties
                    game.Moves = gameState.Moves;
                    game.MatchesFound = gameState.MatchesFound;
                    game.ElapsedTime = gameState.ElapsedTime;
                    game.IsCompleted = gameState.IsCompleted;

                    // In a real implementation, you'd also need to restore the card states

                    return game;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading game: {ex.Message}",
                    "Load Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        public async Task UpdateStatisticsAsync(User user, GameResult result)
        {
            try
            {
                string userStatsFile = Path.Combine(StatisticsDirectoryName, $"{user.Name}.json");
                UserStatistics stats;

                // Load existing statistics if available
                if (File.Exists(userStatsFile))
                {
                    using (FileStream fs = new FileStream(userStatsFile, FileMode.Open))
                    {
                        stats = await JsonSerializer.DeserializeAsync<UserStatistics>(fs);
                    }
                }
                else
                {
                    stats = new UserStatistics { Username = user.Name };
                }

                // Update statistics
                stats.GameResults.Add(result);
                stats.TotalGames++;

                if (result.IsWon)
                {
                    stats.GamesWon++;

                    // Update best time for this category and board size
                    var key = $"{result.Category}_{result.Rows}x{result.Columns}";
                    if (!stats.BestTimes.ContainsKey(key) ||
                        result.Duration < stats.BestTimes[key])
                    {
                        stats.BestTimes[key] = result.Duration;
                    }

                    // Update best moves for this category and board size
                    if (!stats.BestMoves.ContainsKey(key) ||
                        result.Moves < stats.BestMoves[key])
                    {
                        stats.BestMoves[key] = result.Moves;
                    }
                }

                // Save updated statistics
                using (FileStream fs = new FileStream(userStatsFile, FileMode.Create))
                {
                    await JsonSerializer.SerializeAsync(fs, stats);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error updating statistics: {ex.Message}",
                    "Statistics Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    // Helper classes for serialization
    public class GameState
    {
        public string PlayerName { get; set; }
        public string Category { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Moves { get; set; }
        public int MatchesFound { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public bool IsCompleted { get; set; }
        public System.Collections.Generic.List<MemoryCard> Cards { get; set; }
    }

    public class UserStatistics
    {
        public string Username { get; set; }
        public int TotalGames { get; set; }
        public int GamesWon { get; set; }
        public System.Collections.Generic.List<GameResult> GameResults { get; set; } = new System.Collections.Generic.List<GameResult>();
        public System.Collections.Generic.Dictionary<string, TimeSpan> BestTimes { get; set; } = new System.Collections.Generic.Dictionary<string, TimeSpan>();
        public System.Collections.Generic.Dictionary<string, int> BestMoves { get; set; } = new System.Collections.Generic.Dictionary<string, int>();
    }
}