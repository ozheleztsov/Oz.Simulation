using System;
using System.Globalization;
using System.Windows.Data;

namespace Oz.Simulation.Client.Converters;

public class StringToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        double.TryParse(value.ToString(), out var result) ? result : 0.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}