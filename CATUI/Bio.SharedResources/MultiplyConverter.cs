using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace Bio.SharedResources
{
    public class MultiplyConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.OfType<double>().Aggregate(1.0, (current, t) => current*t);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new MultiplyConverter();
        }
    }  
}


