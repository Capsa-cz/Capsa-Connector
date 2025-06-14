using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Capsa_Connector.Controller.Tools;

public class BoolToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            var styles = parameter as object[];
            if (styles != null && styles.Length == 2)
            {
                return boolValue ? styles[0] : styles[1];
            }
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}