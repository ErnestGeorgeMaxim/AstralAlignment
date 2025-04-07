using System;

namespace AstralAlignment.Models
{
    public class GameResult
    {
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Moves { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsWon { get; set; }
    }
}