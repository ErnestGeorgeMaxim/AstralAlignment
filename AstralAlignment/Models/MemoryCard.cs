using System;

namespace AstralAlignment.Models
{
    public class MemoryCard
    {
        public string Value { get; }
        public string Id { get; }
        public bool IsFlipped { get; private set; }
        public bool IsMatched { get; private set; }

        // Added property to help with image paths
        public string ImagePath { get; }

        public MemoryCard(string value)
        {
            Value = value;
            Id = Guid.NewGuid().ToString(); // Unique identifier for each card
            IsFlipped = false;
            IsMatched = false;

            // Can be used if needed, but we're handling paths in the ViewModel
            ImagePath = $"{Value}.png";
        }

        public MemoryCard(string value, string id, bool isFlipped = false, bool isMatched = false)
        {
            Value = value;
            Id = id ?? Guid.NewGuid().ToString();
            IsFlipped = isFlipped;
            IsMatched = isMatched;
            ImagePath = $"{Value}.png";
        }

        public void Flip()
        {
            if (!IsMatched)
            {
                IsFlipped = !IsFlipped;
            }
        }

        public void SetMatched()
        {
            IsMatched = true;
            IsFlipped = true;
        }

        public bool MatchesWith(MemoryCard other)
        {
            return Value == other.Value;
        }
    }
}