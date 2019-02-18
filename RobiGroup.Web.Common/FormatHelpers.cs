using System.Collections.Generic;
using System.IO;

namespace RobiGroup.Web.Common
{
    public class FormatHelpers
    {
        public static readonly HashSet<string> ImageExtensions = new HashSet<string>{ ".jpeg", ".jpg", ".png", ".bmp", ".gif" };
        public static readonly HashSet<string> AudioExtensions = new HashSet<string>{ ".mp3", ".wav" };

        public static string NormalizePhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length == 10)
            {
                if (phoneNumber.StartsWith("7"))
                {
                    return "7" + phoneNumber;
                }
            }
            else if (phoneNumber.Length == 11)
            {
                if (phoneNumber.StartsWith("77"))
                {
                    return phoneNumber;
                }
                else if (phoneNumber.StartsWith("87"))
                {
                    return "77" + phoneNumber.Remove(0, 2);
                }
            }
            else if (phoneNumber.StartsWith("+"))
            {
                return phoneNumber.Remove(0, 1);
            }

            return phoneNumber;
        }

        public static bool IsImageFile(string file)
        {
            var extension = Path.GetExtension(file);
            return ImageExtensions.Contains(extension.ToLower());
        }

        public static bool IsAudioFile(string file)
        {
            var extension = Path.GetExtension(file);
            return AudioExtensions.Contains(extension);
        }
    }
}