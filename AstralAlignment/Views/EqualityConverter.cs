using System;
using System.Globalization;
using System.Windows.Data;

namespace AstralAlignment.Views
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert the value to int for comparison if possible
            if (value is int intValue && parameter is string paramString)
            {
                if (int.TryParse(paramString, out int paramValue))
                {
                    return intValue == paramValue;
                }
            }

            // Compare both as strings if above conversion fails
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handle conversion back from IsSelected to the actual value
            if (value is bool isSelected && isSelected && parameter != null)
            {
                if (targetType == typeof(int) && parameter is string paramStr)
                {
                    if (int.TryParse(paramStr, out int result))
                    {
                        return result;
                    }
                }
                return parameter;
            }
            return null;
        }
    }
}