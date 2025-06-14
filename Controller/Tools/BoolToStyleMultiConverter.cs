using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Capsa_Connector.Controller.Tools
{
    public class BoolToStyleMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool boolValue && values[1] is Style activeStyle && values[2] is Style inactiveStyle)
            {
                return boolValue ? activeStyle : inactiveStyle;
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}