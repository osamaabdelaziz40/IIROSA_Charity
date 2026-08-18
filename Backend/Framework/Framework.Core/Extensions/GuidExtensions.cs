// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuidExtensions.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Extensions
{
    #region usings

    using System;

    #endregion usings

    /// <summary>
    ///     The guid extensions.
    /// </summary>
    public static class GuidExtensions
    {
        // http://stackoverflow.com/questions/211498/is-there-a-net-equalent-to-sql-servers-newsequentialid
        // NHibernate.Id.GuidCombGenerator
        /// <summary>
        /// The as sequential guid.
        /// </summary>
        /// <param name="guid">
        /// The guid.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public static Guid AsSequentialGuid(this Guid guid)
        {
            var guidArray = guid.ToByteArray();

            var baseDate = new DateTime(1900, 1, 1);
            var now = DateTime.Now;

            // Get the days and milliseconds which will be used to build the byte string
            var days = new TimeSpan(now.Ticks - baseDate.Ticks);
            var msecs = now.TimeOfDay;

            // Convert to a byte array
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333
            var daysArray = BitConverter.GetBytes(days.Days);
            var msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }

        public static bool IsNotNullOrEmpty(this Guid? val)
        {
            if (val == null || val == Guid.Empty)
                return false;
            return true;
        }

        public static bool IsNotNullOrEmpty(this Guid val)
        {
            if (val == Guid.Empty)
                return false;
            return true;
        }
    }
}