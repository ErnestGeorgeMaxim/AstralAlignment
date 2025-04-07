using AstralAlignment.Models;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AstralAlignment.ViewModels
{
    public class MemoryCardViewModel : INotifyPropertyChanged
    {
        private readonly MemoryCard _card;
        private bool _isFlipped;
        private bool _isMatched;

        public string Value => _card.Value;

        // Generate image path based on the card value
        public string ImagePath => $"/Images/Cards/{_card.Value.ToLower().Replace(" ", "-")}.png";

        // Create ImageSource for binding
        public ImageSource ImageSource => new BitmapImage(new System.Uri(ImagePath, System.UriKind.Relative));

        public bool IsFlipped
        {
            get => _isFlipped;
            private set
            {
                _isFlipped = value;
                OnPropertyChanged(nameof(IsFlipped));
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            private set
            {
                _isMatched = value;
                OnPropertyChanged(nameof(IsMatched));
            }
        }

        public MemoryCardViewModel(MemoryCard card)
        {
            _card = card;
            _isFlipped = card.IsFlipped;
            _isMatched = card.IsMatched;
        }

        public void Flip()
        {
            _card.Flip();
            IsFlipped = _card.IsFlipped;
        }

        public void SetMatched()
        {
            _card.SetMatched();
            IsMatched = true;
            IsFlipped = true;
        }

        public bool MatchesWith(MemoryCardViewModel other)
        {
            return _card.MatchesWith(other._card);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}