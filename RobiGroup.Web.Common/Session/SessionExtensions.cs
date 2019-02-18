using System;
using Microsoft.AspNetCore.Http;

namespace RobiGroup.Web.Common.Session
{
    public static class SessionExtensions
    {
        public static bool? GetBoolean(this ISession session, string key)
        {
            byte[] val;
            return session.TryGetValue(key, out val) ? val[0] == 1 : (bool?)null;
        }

        public static DateTime? GetDateTime(this ISession session, string key)
        {
            byte[] val;
            return session.TryGetValue(key, out val) ? DateTime.FromFileTime(BitConverter.ToInt64(val, 0)) : (DateTime?)null;
        }

        public static long? GetLong(this ISession session, string key)
        {
            byte[] val;
            return session.TryGetValue(key, out val) ? BitConverter.ToInt64(val, 0) : (long?)null;
        }

        public static void Set(this ISession session, string key, bool value)
        {
            session.Set(key, new []{ value ? (byte)1 : (byte)0 });
        }

        public static void Set(this ISession session, string key, DateTime value)
        {
            session.Set(key, BitConverter.GetBytes(value.ToFileTime()));
        }

        public static void Set(this ISession session, string key, long value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }
    }
}