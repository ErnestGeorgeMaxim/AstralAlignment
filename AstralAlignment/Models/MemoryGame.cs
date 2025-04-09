using AstralAlignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace AstralAlignment.Models
{
    public class MemoryGame
    {
        public User Player { get; }
        public string Category { get; }
        public int Rows { get; }
        public int Columns { get; }
        public List<MemoryCard> Cards { get; }
        public int Moves { get; set; }
        public int MatchesFound { get; set; }
        public DateTime StartTime { get; private set; }
        public TimeSpan ElapsedTime { get; set; }
        public bool IsCompleted { get; set; }

        // New properties for time limit
        public TimeSpan TimeLimit { get; }
        public TimeSpan RemainingTime => TimeLimit - ElapsedTime;
        public bool IsTimeExpired => ElapsedTime >= TimeLimit;

        // Constructor with time limit parameter
        public MemoryGame(User player, string category, int rows, int columns, TimeSpan timeLimit)
        {
            Player = player;
            Category = category;
            Rows = rows;
            Columns = columns;
            Cards = new List<MemoryCard>();
            Moves = 0;
            MatchesFound = 0;
            StartTime = DateTime.Now;
            IsCompleted = false;
            TimeLimit = timeLimit;

            // Debug information
            Debug.WriteLine($"Creating new game with dimensions: {rows}x{columns} and time limit: {timeLimit.TotalMinutes} minutes");

            // Initialize the game board
            InitializeCards();

            // Debug confirmation
            Debug.WriteLine($"Game initialized with {Cards.Count} cards");
        }

        // Constructor overload for backward compatibility
        public MemoryGame(User player, string category, int rows, int columns)
            : this(player, category, rows, columns, TimeSpan.FromMinutes(1)) // Default 3 minutes
        {
        }

        // Constructor for restoring a saved game
        public MemoryGame(User player, string category, int rows, int columns, TimeSpan timeLimit, List<MemoryCard> savedCards)
        {
            Player = player;
            Category = category;
            Rows = rows;
            Columns = columns;
            TimeLimit = timeLimit;
            StartTime = DateTime.Now;
            IsCompleted = false;
            Moves = 0;
            MatchesFound = 0;

            // Use the saved cards if provided, otherwise initialize new ones
            if (savedCards != null && savedCards.Count == rows * columns)
            {
                Cards = savedCards;

                // Count matched pairs
                MatchesFound = Cards.Count(c => c.IsMatched) / 2;
                Debug.WriteLine($"Loaded game with {Cards.Count} cards, {MatchesFound} matches found");
            }
            else
            {
                Cards = new List<MemoryCard>();
                InitializeCards();
                Debug.WriteLine($"Created new cards for loaded game");
            }
        }

        // Explicitly update the start time
        public void UpdateStartTime(TimeSpan elapsedTime)
        {
            StartTime = DateTime.Now.Subtract(elapsedTime);
        }

        private void InitializeCards()
        {
            // Create pairs based on the category
            List<string> pairs = GeneratePairs();
            Debug.WriteLine($"Generated {pairs.Count} pairs for a {Rows}x{Columns} grid");

            // Create memory cards for each pair
            foreach (var pairValue in pairs)
            {
                // Create two cards with the same pair value
                Cards.Add(new MemoryCard(pairValue));
                Cards.Add(new MemoryCard(pairValue));
            }

            // Shuffle the cards
            ShuffleCards();

            // Verify we have the correct number of cards
            int expectedCards = Rows * Columns;
            if (Cards.Count != expectedCards)
            {
                Debug.WriteLine($"WARNING: Expected {expectedCards} cards but created {Cards.Count} cards");
            }
        }

        private List<string> GeneratePairs()
        {
            int pairsCount = (Rows * Columns) / 2;
            List<string> pairs = new List<string>();
            Debug.WriteLine($"Generating {pairsCount} pairs for category: {Category}");

            // Generate pairs based on category
            switch (Category)
            {
                case "Zodiac Signs":
                    pairs.AddRange(new List<string> {
                        "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo",
                        "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"
                }.Take(pairsCount));
                    break;
                case "Celestial Bodies":
                    pairs.AddRange(new List<string> {
                        "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter",
                        "Saturn", "Uranus", "Neptune", "Pluto", "Moon", "Comet",
                        "Void", "Sedna", "Deimos", "Eris", "Ceres", "Europa"
                }.Take(pairsCount));
                    break;
                case "Constellations":
                    pairs.AddRange(new List<string> {
                        "UrsaMajor", "Orion", "Cassiopeia", "Draco", "Pegasus", "Perseus",
                        "Andromeda", "Cygnus", "Lyra", "Centaurus", "Hercules", "Aquila",
                        "Delphinus", "Vulpecula", "Lacerta", "Cepheus", "Bootes", "CanisMajor"
                }.Take(pairsCount));
                    break;
                default:
                    // Default to numbers if no category matches
                    for (int i = 1; i <= pairsCount; i++)
                    {
                        pairs.Add(i.ToString());
                    }
                    break;
            }

            // Check for category limitations
            if (pairs.Count < pairsCount)
            {
                Debug.WriteLine($"WARNING: Category {Category} only has {pairs.Count} items but needed {pairsCount}");
                // Add numbered pairs if we don't have enough category items
                for (int i = pairs.Count + 1; i <= pairsCount; i++)
                {
                    pairs.Add($"Extra {i}");
                }
            }

            return pairs;
        }

        private void ShuffleCards()
        {
            // Fisher-Yates shuffle algorithm
            Random rng = new Random();
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                MemoryCard value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }
    }
}