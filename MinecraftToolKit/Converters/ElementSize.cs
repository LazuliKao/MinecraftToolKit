using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Elements.Converters
{
    public class MainWinRectangle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)) : new SolidColorBrush(Color.FromArgb(255, 158, 158, 158));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class DialogHeightByOpen : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? 200 : 0;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
