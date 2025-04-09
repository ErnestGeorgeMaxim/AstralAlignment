using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AstralAlignment.ViewModels;

namespace AstralAlignment.Views
{
    /// <summary>
    /// Interaction logic for StatisticsOverlay.xaml
    /// </summary>
    public partial class StatisticsView : UserControl
    {
        // Event to notify parent when dialog is closed
        public event EventHandler DialogClosed;

        public StatisticsView(StatisticsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Handle key presses
            this.PreviewKeyDown += StatisticsView_PreviewKeyDown;
        }

        private void StatisticsView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                // Close overlay on Escape or Enter
                DialogClosed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}