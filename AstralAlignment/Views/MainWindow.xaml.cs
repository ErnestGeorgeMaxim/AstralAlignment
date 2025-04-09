using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AstralAlignment.ViewModels;
using AstralAlignment.Views;

namespace AstralAlignment.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Reference to the exit confirmation overlay
        private ExitConfirmationDialog exitOverlay;

        // Reference to the statistics overlay
        private StatisticsView statisticsOverlay;

        public MainWindow()
        {
            InitializeComponent();

            // Create the exit overlay
            exitOverlay = new ExitConfirmationDialog();
            exitOverlay.DecisionMade += ExitOverlay_DecisionMade;
            exitOverlay.Visibility = Visibility.Collapsed;

            // Add the overlay to the window's grid
            MainGrid.Children.Add(exitOverlay);

            // Register the KeyDown event at the window level with PreviewKeyDown
            // to ensure it gets priority over other controls
            this.PreviewKeyDown += Window_PreviewKeyDown;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Only handle ESC if the overlay isn't visible
            if (e.Key == Key.Escape && exitOverlay.Visibility != Visibility.Visible
                && (statisticsOverlay == null || statisticsOverlay.Visibility != Visibility.Visible))
            {
                // Mark the event as handled to prevent default behavior
                e.Handled = true;

                // Show the confirmation overlay
                ShowExitConfirmation();
            }
        }

        private void ShowExitConfirmation()
        {
            // First set visibility
            exitOverlay.Visibility = Visibility.Visible;

            // Apply fade-in animation
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            exitOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Set keyboard focus to the overlay so it can handle key presses
            exitOverlay.Focus();
        }

        private void ExitOverlay_DecisionMade(object sender, bool exitConfirmed)
        {
            // Animate out
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            fadeOut.Completed += (s, e) =>
            {
                // Hide the overlay when animation completes
                exitOverlay.Visibility = Visibility.Collapsed;

                if (exitConfirmed)
                {
                    // User confirmed exit
                    Application.Current.Shutdown();
                }
            };

            exitOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        // Method to show statistics overlay
        public void ShowStatisticsOverlay(StatisticsViewModel viewModel)
        {
            // Create statistics overlay if it doesn't exist
            if (statisticsOverlay == null)
            {
                statisticsOverlay = new StatisticsView(viewModel);
                statisticsOverlay.DialogClosed += StatisticsOverlay_DialogClosed;
                statisticsOverlay.Visibility = Visibility.Collapsed;
                MainGrid.Children.Add(statisticsOverlay);
            }
            else
            {
                // Update the data context if the overlay already exists
                statisticsOverlay.DataContext = viewModel;
            }

            // Show the overlay
            statisticsOverlay.Visibility = Visibility.Visible;

            // Apply fade-in animation
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            statisticsOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Set keyboard focus to the overlay so it can handle key presses
            statisticsOverlay.Focus();
        }

        private void StatisticsOverlay_DialogClosed(object sender, EventArgs e)
        {
            // Animate out
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            fadeOut.Completed += (s, args) =>
            {
                // Hide the overlay when animation completes
                statisticsOverlay.Visibility = Visibility.Collapsed;
            };

            statisticsOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}