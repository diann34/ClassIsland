using System;
using ClassIsland.Core.Services;

namespace ClassIsland.Helpers;

public static class TimeSpanFormatHelper
{
    public static string Format(TimeSpan ts)
    {
        var r = "";
        var h = ts.TotalHours;
        var m = ts.Minutes;
        var s = ts.Seconds;

        if (h >= 1)
        {
            r += LocalizationService.Translate(
                "Helpers.TimeSpanFormatHelper.Format.Hours",
                Math.Floor(ts.TotalHours));
        }
        if (m >= 1)
        {
            if (s >= 1)
            {
                r += LocalizationService.Translate("Helpers.TimeSpanFormatHelper.Format.MinutesShort", m);
            }
            else
            {
                r += LocalizationService.Translate("Helpers.TimeSpanFormatHelper.Format.Minutes", m);
            }
        }
        if (s >= 1)
        {
            r += LocalizationService.Translate("Helpers.TimeSpanFormatHelper.Format.Seconds", s);
        }

        return r;
    }
}
