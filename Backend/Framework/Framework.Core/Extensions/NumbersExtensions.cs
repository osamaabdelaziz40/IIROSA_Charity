// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NumbersExtensions.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// Comment By Ali Elrediny
//https://github.com/Humanizr/Humanizer
//Humanizer meets all your .NET needs for manipulating and displaying strings, enums, dates, times, timespans, numbers and quantities

namespace Framework.Core.Extensions
{
    #region usings

    using Framework.Core.Globalization;
    using Framework.Core.Utils;
    using System;
    using System.Text;

    #endregion usings

    /// <summary>
    ///     The numbers extensions.
    /// </summary>
    public static class NumbersExtensions
    {
        /// <summary>
        /// The to arabic digits string.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToArabicDigitsString(this int number)
        {
            var str = number.ToString();
            string[] arabicNumsArr = { "٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩" };
            var sb = new StringBuilder();
            foreach (var t in str)
            {
                sb.AppendFormat("{0}", arabicNumsArr[int.Parse(t.ToString())]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// The to arabic words.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToArabicWords(this decimal number)
        {
            var toWord = new NumberToWord(number, null);
            return toWord.ConvertToArabic();
        }

        public static string ToRiyalSuadi(this double amount)
        {
            return string.Format(System.Globalization.CultureInfo.CreateSpecificCulture("ar-SA"), "{0:c4}", amount);
        }

        public static string ToRyalSuadi(this double amount)
        {
            return string.Format(System.Globalization.CultureInfo.CreateSpecificCulture("ar-SA"), "{0:c4}", amount);
        }

        public static string ToRyalSuadi(this double? amount)
        {
            return string.Format(System.Globalization.CultureInfo.CreateSpecificCulture("ar-SA"), "{0:c4}", amount.HasValue ? amount.Value : 0);
        }

        /// <summary>
        /// The to arabic words.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToArabicWords(this int number)
        {
            var toWord = new NumberToWord(number, new CurrencyInfo(CurrencyInfo.Currencies.GenericNumber), false);
            return toWord.ConvertToArabic();
        }

        /// <summary>
        /// The to english words.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToEnglishWords(this int number)
        {
            if (number == 0)
            {
                return "zero";
            }

            if (number < 0)
            {
                return "minus " + ToEnglishWords(Math.Abs(number));
            }

            var words = string.Empty;

            if (number / 1000000 > 0)
            {
                words += ToEnglishWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if (number / 1000 > 0)
            {
                words += ToEnglishWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if (number / 100 > 0)
            {
                words += ToEnglishWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != string.Empty)
                {
                    words += "and ";
                }

                var unitsMap = new[]
                                   {
                                       "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
                                       "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
                                       "seventeen", "eighteen", "nineteen"
                                   };
                var tensMap = new[]
                                  {
                                      "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty",
                                      "ninety"
                                  };

                if (number < 20)
                {
                    words += unitsMap[number];
                }
                else
                {
                    words += tensMap[number / 10];
                    if (number % 10 > 0)
                    {
                        words += "-" + unitsMap[number % 10];
                    }
                }
            }

            return words;
        }

        /// <summary>
        /// The to english words.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToEnglishWords(this decimal number)
        {
            var toWord = new NumberToWord(number, null);
            return toWord.ConvertToEnglish();
        }

        /// <summary>
        /// The to file size.
        /// </summary>
        /// <param name="size">
        /// The size.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToFileSize(this long size)
        {
            if (size < 1024)
            {
                return size.ToString("F0") + " bytes";
            }

            if (size < Math.Pow(1024, 2))
            {
                return (size / 1024).ToString("F0") + "KB";
            }

            if (size < Math.Pow(1024, 3))
            {
                return (size / Math.Pow(1024, 2)).ToString("F0") + "MB";
            }

            if (size < Math.Pow(1024, 4))
            {
                return (size / Math.Pow(1024, 3)).ToString("F0") + "GB";
            }

            if (size < Math.Pow(1024, 5))
            {
                return (size / Math.Pow(1024, 4)).ToString("F0") + "TB";
            }

            if (size < Math.Pow(1024, 6))
            {
                return (size / Math.Pow(1024, 5)).ToString("F0") + "PB";
            }

            return (size / Math.Pow(1024, 6)).ToString("F0") + "EB";
        }

        /// <summary>
        /// The to words.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToWords(this int number)
        {
            return CultureHelper.IsArabic ? number.ToArabicWords() : number.ToEnglishWords();
        }
    }
}