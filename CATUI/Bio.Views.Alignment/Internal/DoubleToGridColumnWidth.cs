using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Bio.Views.Alignment.Internal
{
    [ValueConversion(typeof(double), typeof(GridLength))]
    public class DoubleToGridColumnWidth : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(double) || targetType != typeof(GridLength))
                return DependencyProperty.UnsetValue;

            double currentValue = (double) value;
            if (currentValue == 0.0)
                return new GridLength(0);
            if (Double.IsPositiveInfinity(currentValue))
                return new GridLength(.5, GridUnitType.Star);
            return new GridLength(currentValue, GridUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(GridLength) || targetType != typeof(double))
                return null;

            GridLength gl = (GridLength) value;
            return gl.Value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new DoubleToGridColumnWidth();
        }
    }
}
