using System;

namespace Overlook.Server.Extensions
{
    public static class DateExtensions
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUtcUnixTimestamp(this DateTime value)
        {
            value = value.ToUniversalTime();
            var elapsedTime = value - _epoch;
            return (long) elapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Converts a DateTime into a comparable (but not 100% exact) date time 
        /// that can be used to compare with query results
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToComparableDateTime(this DateTime value)
        {
            return DateTime.Parse(value.ToString());
        }
    }
}
