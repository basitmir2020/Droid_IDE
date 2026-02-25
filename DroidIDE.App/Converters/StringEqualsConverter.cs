using System.Globalization;

namespace DroidIDE.App.Converters;

/// <summary>
/// Returns <c>true</c> when the bound string value equals the <see cref="IValueConverter.Convert"/>
/// <c>parameter</c>. Used for panel-switching visibility bindings such as
/// <c>IsVisible="{Binding ActivePanel, Converter={StaticResource StringEqualsConverter}, ConverterParameter=Explorer}"</c>.
/// </summary>
public class StringEqualsConverter : IValueConverter
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="value"/> equals <paramref name="parameter"/> (case-ordinal).
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && parameter is string target)
            return string.Equals(str, target, StringComparison.Ordinal);

        return false;
    }

    /// <summary>Not used — one-way binding only.</summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
