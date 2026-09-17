using System.Globalization;
using Avalonia.Data.Converters;
using ClassIsland.Core.Services;
using ClassIsland.Shared.Models.Profile;

namespace ClassIsland.Core.Converters;

/// <summary>
/// 将 <see cref="TimeLayoutItem.TimeType"/> 转换为其对应的名称的值转换器。
/// </summary>
public class TimeTypeToTimeTypeNameConverter : IValueConverter
{
    /// <summary>
    /// 获取 <see cref="TimeTypeToTimeTypeNameConverter"/> 的实例。
    /// </summary>
    public static readonly TimeTypeToTimeTypeNameConverter Instance = new();

    private TimeTypeToTimeTypeNameConverter() {}
    
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int type)
        {
            return "？？？";
        }
        return type switch
        {
            0 => LocalizationService.Translate("Core.Converters.TimeTypeToTimeTypeNameConverter.Class"),
            1 => LocalizationService.Translate("Core.Converters.TimeTypeToTimeTypeNameConverter.Break"),
            2 => LocalizationService.Translate("Core.Converters.TimeTypeToTimeTypeNameConverter.Separator"),
            3 => LocalizationService.Translate("Core.Converters.TimeTypeToTimeTypeNameConverter.Action"),
            _ => "？？？"
        };
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
