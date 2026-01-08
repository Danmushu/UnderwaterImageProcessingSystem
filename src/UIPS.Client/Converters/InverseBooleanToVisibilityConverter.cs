using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UIPS.Client.Converters;

/// <summary>
/// 布尔值反转可见性转换器
/// true -> Collapsed, false -> Visible
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
            return booleanValue ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
