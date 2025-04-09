using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AstralAlignment.Models;

namespace AstralAlignment.Views
{
    /// <summary>
    /// Interaction logic for DeleteUserConfirmationDialog.xaml
    /// </summary>
    public partial class DeleteUserConfirmationDialog : UserControl
    {
        // Event to notify parent about the decision
        public event EventHandler<bool> DecisionMade;

        // Property to store the user being deleted
        private User _userToDelete;

        // Property for message text binding
        public string MessageText { get; private set; }

        public DeleteUserConfirmationDialog(User userToDelete)
        {
            InitializeComponent();

            // Set the DataContext to this instance to enable binding
            this.DataContext = this;

            // Store the user to delete
            _userToDelete = userToDelete;

            // Set the message text
            MessageText = $"Are you sure you want to delete {_userToDelete.Name}?";

            // Handle key presses
            this.PreviewKeyDown += DeleteUserConfirmationDialog_PreviewKeyDown;
        }

        private void DeleteUserConfirmationDialog_PreviewKeyDown(object sender, KeyEventArgs e)
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