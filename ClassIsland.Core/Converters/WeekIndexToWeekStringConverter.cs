using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using ClassIsland.Core.Services;


namespace ClassIsland.Core.Converters;

public class WeekIndexToWeekStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case int index:
                return ToWeek(index);
            case ObservableCollection<int> coll:
                return string.Join(" ", coll.ToList().ConvertAll(ToWeek));
            default:
                return "";
        }

        static string ToWeek(int index) => index switch
        {
            0 => LocalizationService.Translate("Core.Converters.WeekIndexToWeekStringConverter.Sunday"),
            1 => LocalizationService.Translate("Core.Converters.WeekIndexToWeekStringConverter.Monday"),
            2 => LocalizationService.Translate("Core.Converters.WeekIndexToWeekStringConverter.Tuesday"),
            3 => LocalizationService.Translate("Core.Converters.WeekIndexToWeekStringConverter.Wednesday"),
            4 => LocalizationService.Translate("Core.Converters.WeekIndexToWeekStringConverter.Thursday"),
            5 => LocalizationService.Translate("Core.Converters.WeekIndexToWeekStringConverter.Friday"),
            6 => LocalizationService.Translate("Core.Converters.WeekIndexToWeekStringConverter.Saturday"),
            _ => "???"
        };
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
