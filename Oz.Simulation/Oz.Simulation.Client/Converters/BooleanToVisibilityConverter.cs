using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Oz.Simulation.Client.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = (bool)value;
        return isVisible ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException(nameof(BooleanToVisibilityConverter));
}

public class BooleanToVisibilityConverter2 : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = (bool)value;
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException(nameof(BooleanToVisibilityConverter2));
}