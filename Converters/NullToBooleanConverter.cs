using System.Globalization;

namespace FinalYearProject
{
    public sealed class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var hasValue = value is not null;
            return string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase) ? !hasValue : hasValue;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
