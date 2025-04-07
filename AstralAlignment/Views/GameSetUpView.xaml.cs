using System.Windows.Controls;
using AstralAlignment.Models;
using AstralAlignment.ViewModels;

namespace AstralAlignment.Views
{
    public partial class GameSetUpView : UserControl
    {
        // Default parameterless constructor
        public GameSetUpView()
        {
            InitializeComponent();
        }

        // Constructor that takes a User parameter
        public GameSetUpView(User user)
        {
            InitializeComponent();

            // Set up the DataContext with the user parameter
            this.DataContext = new GameSetUpViewModel(user);
        }
    }
}