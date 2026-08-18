// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CultureHelper.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Globalization
{
    #region usings

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using System;
    using System.Globalization;
    using System.Threading;

    #endregion usings

    /// <summary>
    ///     The culture helper.
    /// </summary>
    public static class CultureHelper
    {
        /// <summary>
        /// The current culture name.
        /// </summary>
        public static string CurrentCultureName => Thread.CurrentThread.CurrentCulture.Name;

        /// <summary>
        ///     The current direction.
        /// </summary>
        public static string CurrentDirection => IsRightToLeft ? "rtl" : "ltr";

        /// <summary>
        ///     The current language.
        /// </summary>
        public static string CurrentLanguage => CultureInfo.CurrentCulture.Name.Substring(0, 2);

        /// <summary>
        ///     The is arabic.
        /// </summary>
        public static bool IsArabic => CurrentLanguage.Equals("ar");

        /// <summary>
        ///     The is right to left.
        /// </summary>
        public static bool IsRightToLeft => CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

        /// <summary>
        /// The get culture info.
        /// </summary>
        /// <param name="cultureName">
        /// The culture name.
        /// </param>
        /// <returns>
        /// The <see cref="CultureInfo"/>.
        /// </returns>
        public static CultureInfo GetCultureInfo(string cultureName = "ar-SA")
        {
            // The default date in the system should be gregorian.
            var dateTimeFormat = new CultureInfo("en-GB").DateTimeFormat;

            // The default currency should be SAR
            var numberFormat = new CultureInfo("ar-SA").NumberFormat;
            numberFormat.CurrencyPositivePattern = 3;
            numberFormat.CurrencyNegativePattern = 3;

            // numberFormat.DigitSubstitution = DigitShapes.NativeNational;
            if (cultureName.Contains("|") && cultureName.Contains("="))
            {
                cultureName = cultureName.Split('|')[0].Split('=')[1];
            }

            return new CultureInfo(cultureName) { NumberFormat = numberFormat, DateTimeFormat = dateTimeFormat };
        }

        /// <summary>
        /// The initialize culture from cookie.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public static void InitializeCultureFromCookie(HttpContext context)
        {
            var defaultCulture = "en-GB"; // USAMA READ FROM FRAMEWORK SETTINGS

            // USAMA CHECK THE FORMAT IS AS c='en-UK'|uic='en-US'
            // The cookie format is c=%LANGCODE%|uic=%LANGCODE%, where c is Culture and uic is UICulture
            var cookieVal = context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];

            var cultureToSet = cookieVal ?? defaultCulture;
            var culture = GetCultureInfo(cultureToSet);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            if (cookieVal == null)
            {
                context.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }
        }

        public static void InitializeCultureFromCookieAr(HttpContext context)
        {
            var defaultCulture = "ar-SA"; // USAMA READ FROM FRAMEWORK SETTINGS

            // USAMA CHECK THE FORMAT IS AS c='en-UK'|uic='en-US'
            // The cookie format is c=%LANGCODE%|uic=%LANGCODE%, where c is Culture and uic is UICulture
            var cookieVal = context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];

            var cultureToSet = cookieVal ?? defaultCulture;
            var culture = GetCultureInfo(cultureToSet);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            if (cookieVal == null)
            {
                context.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }
        }
    }
}