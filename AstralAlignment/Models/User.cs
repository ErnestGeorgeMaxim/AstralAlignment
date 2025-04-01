using System;
using System.ComponentModel;

namespace AstralAlignment.Models
{
    [Serializable]
    public class User : INotifyPropertyChanged
    {
        private string _name;
        private string _zodiacSignName;
        private string _zodiacImagePath;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string ZodiacSignName
        {
            get => _zodiacSignName;
            set
            {
                if (_zodiacSignName != value)
                {
                    _zodiacSignName = value;
                    OnPropertyChanged(nameof(ZodiacSignName));
                }
            }
        }

        public string ZodiacImagePath
        {
            get => _zodiacImagePath;
            set
            {
                if (_zodiacImagePath != value)
                {
                    _zodiacImagePath = value;
                    OnPropertyChanged(nameof(ZodiacImagePath));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}