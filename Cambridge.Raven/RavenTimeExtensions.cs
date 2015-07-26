using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    /// <summary>
    /// Extension methods to convert to and from the time format that is used by Raven.
    /// </summary>
    public static class RavenTimeExtensions
    {
        #region Constants
        /// <summary>
        /// The format used by Raven.
        /// </summary>
        public const String RAVEN_TIME_FORMAT = "yyyyMMdd'T'HHmmss'Z'";
        #endregion

        #region ToRavenTime
        /// <summary>
        /// Converts a System.DateTime object to a System.String in the format used by Raven.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String ToRavenTime(this DateTime value)
        {
            return value.ToUniversalTime().ToString(RAVEN_TIME_FORMAT);
        }
        #endregion

        #region FromRavenTime
        /// <summary>
        /// Converts a System.String containing data in the time format used by Raven
        /// to a System.DateTime object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Boolean FromRavenTime(this String value, out DateTime result)
        {
            return DateTime.TryParseExact(
                value, 
                RAVEN_TIME_FORMAT, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal, 
                out result);
        }
        #endregion
    }
}
