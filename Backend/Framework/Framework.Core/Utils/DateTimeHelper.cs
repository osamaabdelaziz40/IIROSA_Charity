// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeHelper.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Utils
{
    #region

    #region usings

    using Framework.Core.Globalization;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    #endregion usings

    #endregion

    /// <summary>
    ///     Group of methods help you to manipulate with DateTime
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        ///     The all formats.
        ///     14/12/1437 02:54:14 م
        /// </summary>
        private static readonly string[] allFormats =
            {
                "dd/MM/yyyy", "dd-MMM-yyyy", "dd-MMMM-yyyy", "yyyy/MM/dd", "yyyy/M/d", "dd/MM/yyyy", "d/M/yyyy",
                "dd/M/yyyy", "d/MM/yyyy", "yyyy-MM-dd", "yyyy-M-d", "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy",
                "yyyy MM dd", "yyyy M d", "dd MM yyyy", "d M yyyy", "dd M yyyy", "d MM yyyy", "dd/MM/yyyy HH:mm:ss",
                "G", "g", "yyyy/MM/dd hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt"
            };

        /// <summary>
        /// The days in arabic.
        /// </summary>
        public static string[] DaysInArabic =>
            new[] { "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت" };

        /// <summary>
        /// Calculates the date ranges total years.
        ///     Created by The Magnificent Essam Mostafa
        /// </summary>
        /// <param name="ranges">
        /// The ranges.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public static double CalculateDateRangesTotalYears(List<DateRange> ranges)
        {
            if (ranges == null)
            {
                return 0;
            }

            if (!ranges.Any())
            {
                return 0;
            }

            var totalIntersectedDates = 0;
            double totalPeriod = 0;

            for (var i = 0; i < ranges.Count; i++)
            {
                totalPeriod += ranges[i].TotalDays;
                for (var y = i + 1; y < ranges.Count; y++)
                {
                    if (y > ranges.Count)
                        break;

                    var minEndDate = Min(ranges[i].EndDate, ranges[y].EndDate);
                    var maxStartDate = Max(ranges[i].StartDate, ranges[y].StartDate);
                    totalIntersectedDates += Math.Max((int)(minEndDate - maxStartDate).TotalDays, 0);
                }
            }

            var totalDaysOfExperience = totalPeriod - totalIntersectedDates;
            var numbOfYears = totalDaysOfExperience / 365;

            return numbOfYears;
        }

        /// <summary>
        /// The get average month difference.
        /// </summary>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        /// <param name="endDate">
        /// The end date.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public static double GetAverageMonthDifference(DateTime startDate, DateTime endDate)
        {
            return startDate.Subtract(endDate).Days / (365.25 / 12);
        }

        /// <summary>
        /// The get um al qura date.
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <param name="dateFormat">
        /// The date format.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCurrentUmAlQuraDateString(DateTime date, string dateFormat = "dd/MM/yyyy")
        {
            var culture = new CultureInfo("ar-SA") { DateTimeFormat = { Calendar = new UmAlQuraCalendar() } };
            return date.ToString(dateFormat, culture);
        }

        /// <summary>
        /// This Method help to get rang from start to end Date as list of DateTime By Ahmed Gaduo
        /// </summary>
        /// <param name="date1">
        /// </param>
        /// <param name="date2">
        /// </param>
        /// <param name="exceptionDays">
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<DateTime> GetDateRange(DateTime date1, DateTime date2, List<DateTime> exceptionDays = null)
        {
            // check if there any exception days
            if (exceptionDays != null && exceptionDays.Count > 0)
#pragma warning disable S108 // Nested blocks of code should not be left empty
            {
            }
#pragma warning restore S108 // Nested blocks of code should not be left empty

            // inject it in list to avoid it
            var allDates = new List<DateTime>();

            for (var date = Min(date1, date2); date <= Max(date1, date2); date = date.AddDays(1))
                allDates.Add(date);

            return allDates;
        }

        /// <summary>
        /// The get gregorean date.
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <param name="dateFormat">
        /// The date format.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetGregoreanDateString(DateTime? date, string dateFormat = "dd/MM/yyyy")
        {
            if (date == null)
                return string.Empty;
            var culture = new CultureInfo("en-GB") { DateTimeFormat = { Calendar = new GregorianCalendar() } };
            return date.Value.ToString(dateFormat, culture);
        }

        /// <summary>
        /// The get month difference.
        /// </summary>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        /// <param name="endDate">
        /// The end date.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetMonthDifference(DateTime startDate, DateTime endDate)
        {
            var monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }

        /// <summary>
        /// The get month names.
        /// </summary>
        /// <param name="calendar">
        /// The calendar.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public static string[] GetMonthNames(Calendar calendar)
        {
            return GetMonthNames(calendar, new CultureInfo(CultureInfo.CurrentCulture.ToString()));
        }

        /// <summary>
        /// The get month names.
        /// </summary>
        /// <param name="calendar">
        /// The calendar.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public static string[] GetMonthNames(Calendar calendar, CultureInfo culture)
        {
            if (calendar is UmAlQuraCalendar && culture.Name.StartsWith("en-"))
            {
                return new[]
                           {
                               "Muharram", "Safar", "Rabi' al-awwal", "Rabi' al-thani", "Jumada al-awwal",
                               "Jumada al-thani", "Rajab", "Sha'aban", "Ramadan", "Shawwal", "Dhu al-Qi'dah",
                               "Dhu al-Hijjah"
                           };
            }

            culture.DateTimeFormat.Calendar = calendar;
            var monthNames = culture.DateTimeFormat.MonthNames;
            Array.Resize(ref monthNames, 12);
            return monthNames;
        }

        /// <summary>
        ///     return array of 12 months
        /// </summary>
        /// <returns>
        ///     The <see cref="string[]" />.
        /// </returns>
        public static string[] GetMonthsNumbers()
        {
            return new[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
        }

        /// <summary>
        /// This Method help to get rang from start to end Time as list of TimeSpan By Ahmed Gaduo
        /// </summary>
        /// <param name="time1">
        /// </param>
        /// <param name="time2">
        /// </param>
        /// <param name="minutesPerHour">
        /// Number of minutes between times
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<TimeSpan> GetTimeRange(TimeSpan time1, TimeSpan time2, int minutesPerHour = 30)
        {
            var times = new List<TimeSpan>();
            var lenght =
                ((Max(time1, time2) - Min(time1, time2)).TotalMinutes
                 / minutesPerHour); // because we use time every 30 minutes
            for (var i = 0; i < lenght; i++)
            {
                var lastTime = times.Count == 0 ? new TimeSpan() : times[i - 1];
                times.Add(times.Count == 0 ? Min(time1, time2) : lastTime.Add(new TimeSpan(0, 30, 0)));
            }

            times.Add(Max(time1, time2));
            return times;
        }

        /// <summary>
        /// Replace Hijry month name with its english name
        /// </summary>
        /// <param name="month">
        /// index of month (start from 1)
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetUmmAlquraMonthName(int month)
        {
            var monthsNames = CultureHelper.IsArabic
                                  ? GetMonthNames(new UmAlQuraCalendar(), new CultureInfo("ar-SA"))
                                  : GetMonthNames(new UmAlQuraCalendar(), new CultureInfo("en-US"));
            return monthsNames[month - 1];
        }

        /// <summary>
        /// return array of min-max years
        /// </summary>
        /// <param name="min">
        /// </param>
        /// <param name="max">
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetYearsRange(int min, int max)
        {
            if (min > max)
            {
                throw new Exception("The min parameter must be less than max parameter");
            }

            var years = new List<string>();
            for (var i = min; i <= max; i++)
            {
                years.Add(i.ToString());
            }

            return years;
        }

        /// <summary>
        /// The is gregorian.
        /// </summary>
        /// <param name="gregStr">
        /// The greg str.
        /// </param>
        /// <param name="dateFormat">
        /// The date Format.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsGregorian(string gregStr, string dateFormat = null)
        {
            var formats = !string.IsNullOrEmpty(dateFormat) ? new[] { dateFormat } : allFormats;

            var englishCultureInfo = new CultureInfo("en-GB");

            var cal = new GregorianCalendar();

            if (!string.IsNullOrEmpty(gregStr) && gregStr.Trim().Length > 0)
            {
                try
                {
                    // DateTime tmp = DateTime.Now; ;
                    // return DateTime.TryParse(gregStr, out tmp);
                    var tempDate = DateTime.ParseExact(
                        gregStr,
                        formats,
                        englishCultureInfo.DateTimeFormat,
                        DateTimeStyles.AllowWhiteSpaces);
                    if (tempDate.Year >= cal.MinSupportedDateTime.Year
                        && tempDate.Year <= cal.MaxSupportedDateTime.Year)
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// The is um alqura.
        /// </summary>
        /// <param name="hijriDateStr">
        /// The hijri date str.
        /// </param>
        /// <param name="dateFormat">
        /// The date Format.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsUmAlqura(string hijriDateStr, string dateFormat = null)
        {
            var formats = !string.IsNullOrEmpty(dateFormat) ? new[] { dateFormat } : allFormats;

            var arCul = new CultureInfo("ar-SA");
            var cal = new UmAlQuraCalendar();
            arCul.DateTimeFormat.Calendar = cal;

            if (string.IsNullOrEmpty(hijriDateStr) || hijriDateStr.Trim().Length <= 0)
            {
                return false;
            }

            try
            {
                var tempDate = DateTime.ParseExact(
                    hijriDateStr,
                    formats,
                    arCul.DateTimeFormat,
                    DateTimeStyles.AllowWhiteSpaces);
                if (tempDate.Year >= cal.MinSupportedDateTime.Year && tempDate.Year <= cal.MaxSupportedDateTime.Year)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The max.
        /// </summary>
        /// <param name="date1">
        /// The date 1.
        /// </param>
        /// <param name="date2">
        /// The date 2.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public static DateTime Max(DateTime date1, DateTime date2)
        {
            if (date1 > date2)
                return date1;
            return date2;
        }

        /// <summary>
        /// The max.
        /// </summary>
        /// <param name="time1">
        /// The time 1.
        /// </param>
        /// <param name="time2">
        /// The time 2.
        /// </param>
        /// <returns>
        /// The <see cref="TimeSpan"/>.
        /// </returns>
        public static TimeSpan Max(TimeSpan time1, TimeSpan time2)
        {
            if (time1 > time2)
                return time1;
            return time2;
        }

        /// <summary>
        /// The min.
        /// </summary>
        /// <param name="date1">
        /// The date 1.
        /// </param>
        /// <param name="date2">
        /// The date 2.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public static DateTime Min(DateTime date1, DateTime date2)
        {
            if (date1 < date2)
                return date1;
            return date2;
        }

        /// <summary>
        /// The min.
        /// </summary>
        /// <param name="time1">
        /// The time 1.
        /// </param>
        /// <param name="time2">
        /// The time 2.
        /// </param>
        /// <returns>
        /// The <see cref="TimeSpan"/>.
        /// </returns>
        public static TimeSpan Min(TimeSpan time1, TimeSpan time2)
        {
            if (time1 < time2)
                return time1;
            return time2;
        }

        /// <summary>
        /// The parse gregorian date.
        /// </summary>
        /// <param name="gregStr">
        /// The greg str.
        /// </param>
        /// <param name="dateFormat">
        /// The date Format.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime?"/>.
        /// </returns>
        public static DateTime? ParseGregorianDate(string gregStr, string dateFormat = null)
        {
            var formats = !string.IsNullOrEmpty(dateFormat) ? new[] { dateFormat } : allFormats;

            var englishCultureInfo = new CultureInfo("en-GB");
            var cal = new GregorianCalendar();

            DateTime? toReturn = null;
            if (!string.IsNullOrEmpty(gregStr) && gregStr.Trim().Length > 0)
            {
                try
                {
                    // DateTime tmp = DateTime.Now; ;
                    // return DateTime.TryParse(gregStr, out tmp);
                    toReturn = DateTime.ParseExact(
                        gregStr,
                        formats,
                        englishCultureInfo.DateTimeFormat,
                        DateTimeStyles.AllowWhiteSpaces);
                    if (toReturn.Value.Year >= cal.MinSupportedDateTime.Year
                        && toReturn.Value.Year <= cal.MaxSupportedDateTime.Year)
                    {
                        return toReturn.Value;
                    }

                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// The parse um alqura date.
        /// </summary>
        /// <param name="hijriDateStr">
        /// The hijri date str.
        /// </param>
        /// <param name="dateFormat">
        /// The date Format.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime?"/>.
        /// </returns>
        public static DateTime? ParseUmAlquraDate(string hijriDateStr, string dateFormat = null)
        {
            var formats = !string.IsNullOrEmpty(dateFormat) ? new[] { dateFormat } : allFormats;

            var umAlQuraCal = new UmAlQuraCalendar();
            DateTime? toRet = null;

            if (!string.IsNullOrEmpty(hijriDateStr))
            {
                var arCult = new CultureInfo("ar-SA") { DateTimeFormat = { Calendar = umAlQuraCal } };
                toRet = DateTime.ParseExact(
                    hijriDateStr,
                    formats,
                    arCult.DateTimeFormat,
                    DateTimeStyles.AllowWhiteSpaces);
            }

            return toRet;
        }

        /// <summary>
        /// For calculating age
        /// </summary>
        /// <param name="dateOfBirth">
        /// Enter Date of Birth to Calculate the age
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <returns>
        /// years, months,days, hours...
        /// </returns>
        public static string CalculateAge(DateTime dateOfBirth, out int years)
        {
            var now = DateTime.Now;
            years = new DateTime(DateTime.Now.Subtract(dateOfBirth).Ticks).Year - 1;
            var pastYearDate = dateOfBirth.AddYears(years);
            var months = 0;
            for (var i = 1; i <= 12; i++)
            {
                if (pastYearDate.AddMonths(i) == now)
                {
                    months = i;
                    break;
                }
                else if (pastYearDate.AddMonths(i) >= now)
                {
                    months = i - 1;
                    break;
                }
            }

            var days = now.Subtract(pastYearDate.AddMonths(months)).Days;
            var hours = now.Subtract(pastYearDate).Hours;
            var minutes = now.Subtract(pastYearDate).Minutes;
            var seconds = now.Subtract(pastYearDate).Seconds;
            return $"Age: {years} Year(s) {months} Month(s) {days} Day(s) {hours} Hour(s) {minutes} Minute(s) {seconds} Second(s)";
        }

       
    }
}