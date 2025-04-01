using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AstralAlignment.Services;
using AstralAlignment.ViewModels;

namespace AstralAlignment.Views
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            // Add the converter as a resource before initializing the component
            Resources.Add("BooleanToVisibilityConverter", new BooleanToVisibilityConverter());
            InitializeComponent();

            // Get the MainWindowViewModel instance to access its commands
            var mainVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            if (mainVM != null)
            {
                var userDataService = new UserDataService();
                DataContext = new ProfileViewModel(userDataService);
            }
            else
            {
                // Fallback if MainWindowViewModel isn't accessible
                var userDataService = new UserDataService();
                var fallbackCommand = new AstralAlignment.Commands.RelayCommand(_ =>
                    MessageBox.Show("Unable to navigate to startup screen"));
                DataContext = new ProfileViewModel(userDataService);
            }
        }

    }

    // Nested converter class
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}