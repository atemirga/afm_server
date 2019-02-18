using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace RobiGroup.Web.Common
{
    public static class DateTimeHelpers
    {

        public static long GetTimeInMiliseconds(this DateTime dateTime)
        {
            return (long) dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static long GetTimeInMiliseconds(this DateTime? dateTime)
        {
            return dateTime.HasValue ? GetTimeInMiliseconds(dateTime.Value) : 0;
        }

        public static string ToHumanizedString(this DateTime dateTime, IStringLocalizer _localizer)
        {
            if ((DateTime.Now.Date - dateTime.Date).Days == 0)
            {
                return dateTime.ToString(_localizer["TodayAt"] + " H:mm");
            }
            if ((DateTime.Now.Date - dateTime.Date).Days == 1)
            {
                return dateTime.ToString(_localizer["YesterdayAt"] + " H:mm");
            }

            return dateTime.ToString("dd MMMM " + _localizer["DateTimeAt"] + " H:mm");
        }

        public static string ToHumanizedShortString(this TimeSpan time, IStringLocalizer _localizer)
        {
            string str = "";
            if (time.Days > 0)
            {
                str += " " + _localizer["Time_Short_Days", time.Days];
            }
            if (time.Hours > 0)
            {
                str += " " + _localizer["Time_Short_Hours", time.Hours];
            }
            if (time.Minutes > 0)
            {
                str += " " + _localizer["Time_Short_Minutes", time.Minutes];
            }
            if (time.Seconds > 0)
            {
                str += " " + _localizer["Time_Short_Seconds", time.Seconds];
            }

            return str;
        }

        public static string ToHoursPeriods(IEnumerable<DateTime> times)
        {
            var first = times.FirstOrDefault();

            string period = first.ToString("HH:mm");

            DateTime prev = first;
            foreach (var time in times.Skip(1))
            {
                if (time.Hour - prev.Hour > 1)
                {
                    
                }
            }

            return period;
        }
    }
}
