using AstralAlignment.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace AstralAlignment.Services
{
    #region Game Service Implementation

    public class GameService
    {
        private const string StatisticsDirectoryName = "Statistics";
        private const string SavedGamesDirectoryName = "SavedGames";

        public GameService()
        {
            // Ensure directories exist
            if (!Directory.Exists(StatisticsDirectoryName))
            {
                Directory.CreateDirectory(StatisticsDirectoryName);
            }

            if (!Directory.Exists(SavedGamesDirectoryName))
            {
                Directory.CreateDirectory(SavedGamesDirectoryName);
            }
        }

        public async Task SaveGameAsync(MemoryGame game, string filePath)
        {
            try
            {
                // Create card state objects to save the state of each card
                var cardStates = game.Cards.Select(card => new CardState
                {
                    Value = card.Value,
                    Id = card.Id,
                    IsFlipped = card.IsFlipped,
                    IsMatched = card.IsMatched
                }).ToList();

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
                    TimeLimit = game.TimeLimit,
                    IsCompleted = game.IsCompleted,
                    CardStates = cardStates,
                    SavedAt = DateTime.Now
                };

                // Serialize and save
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    await JsonSerializer.SerializeAsync(fs, gameState, options);
                }

                Debug.WriteLine($"Game saved successfully to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving game: {ex}");
                MessageBox.Show($"Error saving game: {ex.Message}",
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<MemoryGame> LoadGameAsync(string filePath, User currentUser)
        {
            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var gameState = JsonSerializer.Deserialize<GameState>(json, options);

                if (gameState == null)
                {
                    throw new Exception("Could not deserialize the game state.");
                }

                // Verify the game belongs to the current user
                if (gameState.PlayerName != currentUser.Name)
                {
                    throw new Exception("This saved game belongs to another user.");
                }

                Debug.WriteLine($"Loading game: {gameState.Category} {gameState.Rows}x{gameState.Columns}");
                Debug.WriteLine($"Card states count: {gameState.CardStates?.Count ?? 0}");

                // Recreate the exact cards from the saved state
                var restoredCards = new List<MemoryCard>();

                if (gameState.CardStates != null && gameState.CardStates.Count > 0)
                {
                    foreach (var cardState in gameState.CardStates)
                    {
                        // Create a new card with the saved value and ID
                        var card = new MemoryCard(cardState.Value, cardState.Id);

                        // Restore the card state
                        if (cardState.IsFlipped && !card.IsFlipped)
                        {
                            card.Flip();
                        }

                        if (cardState.IsMatched && !card.IsMatched)
                        {
                            card.SetMatched();
                        }

                        restoredCards.Add(card);
                    }

                    Debug.WriteLine($"Restored {restoredCards.Count} cards");
                }

                // Create a new game with our restored cards
                var game = new MemoryGame(
                    currentUser,
                    gameState.Category,
                    gameState.Rows,
                    gameState.Columns,
                    gameState.TimeLimit,
                    restoredCards // Pass the restored cards
                );

                // Update game state
                game.Moves = gameState.Moves;
                game.MatchesFound = gameState.MatchesFound;
                game.ElapsedTime = gameState.ElapsedTime;
                game.IsCompleted = gameState.IsCompleted;

                // Update the start time to account for elapsed time
                game.UpdateStartTime(gameState.ElapsedTime);

                return game;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading game: {ex.Message}",
                    "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error loading game: {ex}");
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
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    await JsonSerializer.SerializeAsync(fs, stats, options);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating statistics: {ex.Message}",
                    "Statistics Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion

    #region Helper Classes

    // Helper classes for serialization
    public class CardState
    {
        public string Value { get; set; }
        public string Id { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
    }

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
        public TimeSpan TimeLimit { get; set; }
        public bool IsCompleted { get; set; }
        public List<CardState> CardStates { get; set; } = new List<CardState>();
        public DateTime SavedAt { get; set; }
    }

    public class UserStatistics
    {
        public string Username { get; set; }
        public int TotalGames { get; set; }
        public int GamesWon { get; set; }
        public List<GameResult> GameResults { get; set; } = new List<GameResult>();
        public Dictionary<string, TimeSpan> BestTimes { get; set; } = new Dictionary<string, TimeSpan>();
        public Dictionary<string, int> BestMoves { get; set; } = new Dictionary<string, int>();
    }

    #endregion
}