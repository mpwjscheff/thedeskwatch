using System.Globalization;

namespace TheDeskWatch.MobileApp.Converters;

public sealed class HexStringToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is not string hexString || string.IsNullOrWhiteSpace(hexString))
        {
            return Colors.Gray;
        }

        try
        {
            return Color.FromArgb(hexString);
        }
        catch (ArgumentException)
        {
            return Colors.Gray;
        }
        catch (FormatException)
        {
            return Colors.Gray;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        => throw new NotImplementedException();
}
