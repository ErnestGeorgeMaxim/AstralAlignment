using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AstralAlignment.Views
{
    /// <summary>
    /// Interaction logic for ExitConfirmationOverlay.xaml
    /// </summary>
    public partial class ExitConfirmationDialog : UserControl
    {
        // Event to notify parent about the decision
        public event EventHandler<bool> DecisionMade;

        public ExitConfirmationDialog()
        {
            InitializeComponent();

            // Handle key presses
            this.PreviewKeyDown += ExitConfirmationDialog_PreviewKeyDown;
        }

        private void ExitConfirmationDialog_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Treat Escape as No
                DecisionMade?.Invoke(this, false);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                // Treat Enter as Yes
                DecisionMade?.Invoke(this, true);
                e.Handled = true;
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DecisionMade?.Invoke(this, true);
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DecisionMade?.Invoke(this, false);
        }
    }
}