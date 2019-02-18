using Newtonsoft.Json.Converters;

namespace RobiGroup.Web.Common
{
    public class CustomDateTimerConverter : IsoDateTimeConverter
    {
        public CustomDateTimerConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}