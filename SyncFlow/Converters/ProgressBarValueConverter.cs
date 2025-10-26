using System.Globalization;
using System.Windows.Data;

namespace SyncFlow.Converters;

public class ProgressBarValueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 4 ||
            !double.TryParse(values[0]?.ToString(), out double value) ||
            !double.TryParse(values[1]?.ToString(), out double minimum) ||
            !double.TryParse(values[2]?.ToString(), out double maximum) ||
            !double.TryParse(values[3]?.ToString(), out double actualWidth))
        {
            return 0.0;
        }

        if (maximum == minimum)
            return 0.0;

        return (value - minimum) * actualWidth / (maximum - minimum);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}