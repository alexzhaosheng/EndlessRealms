using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Gui.Converter;
public class BoolToVisibilityConverter : IValueConverter
{
    public bool NullIsTrue { get; set; }
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return NullIsTrue;
        }
        else
        {
            return !NullIsTrue;
        }
        
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
