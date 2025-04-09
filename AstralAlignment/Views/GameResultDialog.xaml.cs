using AstralAlignment.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AstralAlignment.Views
{
    /// <summary>
    /// Interaction logic for GameResultDialog.xaml
    /// </summary>
    public partial class GameResultDialog : UserControl
    {
        // Event to notify parent about the decision
        public event EventHandler<GameResultAction> ActionSelected;

        public enum GameResultAction
        {
            NewGame,
            Exit
        }

        public GameResultDialog(bool isWon, string timeDisplay, int moves, string matchesDisplay)
        {
            InitializeComponent();

            // Configure UI based on game result
            ConfigureForResult(isWon, timeDisplay, moves, matchesDisplay);

            // Handle key presses
            this.PreviewKeyDown += GameResultDialog_PreviewKeyDown;
        }

        private void ConfigureForResult(bool isWon, string timeDisplay, int moves, string matchesDisplay)
        {
            if (isWon)
            {
                // Victory display
                HeaderText.Text = "VICTORY";
                MessageText.Text = "The stars align in your favor" +
                    "               destiny is yours!";
            }
            else
            {
                // Loss display
                HeaderText.Text = "TIME'S UP";
                MessageText.Text = "The cosmos falls silent... " +
                                   "your alignment has failed.";
            }

            // Set game statistics
            TimeText.Text = timeDisplay;
            MovesText.Text = moves.ToString();
            MatchesText.Text = matchesDisplay;
        }

        private void GameResultDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Treat Escape as Exit
                ActionSelected?.Invoke(this, GameResultAction.Exit);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                // Treat Enter as New Game
                ActionSelected?.Invoke(this, GameResultAction.NewGame);
                e.Handled = true;
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            ActionSelected?.Invoke(this, GameResultAction.NewGame);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            ActionSelected?.Invoke(this, GameResultAction.Exit);
        }
    }
}