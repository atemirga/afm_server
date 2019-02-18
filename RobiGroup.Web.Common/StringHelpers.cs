using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RobiGroup.Web.Common
{
    public static class StringHelpers
    {
        public static string[] SplitToArray(this string str, char delim = ',')
        {
            return str == null ? new string[0] : str.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static int[] SplitToIntArray(this string str, char delim = ',')
        {
            return Array.ConvertAll(SplitToArray(str, delim), int.Parse);
        }


        public static string Slugify(this string s)
        {
            s = s.ToLower();
            s = s.Replace("-", " ");
            s = Regex.Replace(s, "[^а-яa-z0-9\\s-]", " ");
            s = Regex.Replace(s, "\\s+", " ").Trim();
            s = s.Replace(" ", "-");
            return s;
        }

        public static string FirstCharToLower(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToLower() + input.Substring(1);
            }
        }

        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}
