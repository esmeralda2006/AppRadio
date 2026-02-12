using System.Globalization;

namespace RadioFreeDAM.App.Converters;

public class PlayPauseGlyphConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2)
        {
            bool isPlaying = values[0] is bool playing && playing;
            bool isBuffering = values[1] is bool buffering && buffering;

            if (isBuffering)
            {
                return ""; // Vacío cuando está buffering (se muestra ActivityIndicator)
            }

            return isPlaying ? "\ue034" : "\ue037"; // pause : play_arrow
        }

        return "\ue037"; // play_arrow por defecto
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
