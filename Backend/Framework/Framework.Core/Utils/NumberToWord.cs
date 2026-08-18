// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NumberToWord.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Utils
{
    #region usings

    using System;
    using System.Globalization;

    #endregion usings

    /// <summary>
    ///     http://www.codeproject.com/Articles/112949/Number-To-Word-Arabic-Version
    /// </summary>
    public class NumberToWord
    {
        /// <summary>
        ///     The arabic appended group.
        /// </summary>
        private static readonly string[] ArabicAppendedGroup =
            {
                string.Empty, "ألفاً", "مليوناً", "ملياراً", "تريليوناً", "كوادريليوناً", "كوينتليوناً", "سكستيليوناً"
            };

        /// <summary>
        ///     The arabic appended twos.
        /// </summary>
        private static readonly string[] ArabicAppendedTwos =
            {
                "مئتا", "ألفا", "مليونا", "مليارا", "تريليونا", "كوادريليونا", "كوينتليونا", "سكستيليونا"
            };

        /// <summary>
        ///     The arabic feminine ones.
        /// </summary>
        private static readonly string[] ArabicFeminineOnes =
            {
                string.Empty, "إحدى", "اثنتان", "ثلاث", "أربع", "خمس", "ست", "سبع", "ثمان", "تسع", "عشر", "إحدى عشرة",
                "اثنتا عشرة", "ثلاث عشرة", "أربع عشرة", "خمس عشرة", "ست عشرة", "سبع عشرة", "ثماني عشرة", "تسع عشرة"
            };

        /// <summary>
        ///     The arabic group.
        /// </summary>
        private static readonly string[] ArabicGroup =
            {
                "مائة", "ألف", "مليون", "مليار", "تريليون", "كوادريليون", "كوينتليون", "سكستيليون"
            };

        /// <summary>
        ///     The arabic hundreds.
        /// </summary>
        private static readonly string[] ArabicHundreds =
            {
                string.Empty, "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة",
                "تسعمائة"
            };

        /// <summary>
        ///     The arabic ones.
        /// </summary>
        private static readonly string[] ArabicOnes =
            {
                string.Empty, "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة", "عشرة",
                "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر",
                "تسعة عشر"
            };

        /// <summary>
        ///     The arabic plural groups.
        /// </summary>
        private static readonly string[] ArabicPluralGroups =
            {
                string.Empty, "آلاف", "ملايين", "مليارات", "تريليونات", "كوادريليونات", "كوينتليونات", "سكستيليونات"
            };

        /// <summary>
        ///     The arabic tens.
        /// </summary>
        private static readonly string[] ArabicTens =
            {
                "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"
            };

        /// <summary>
        ///     The arabic twos.
        /// </summary>
        private static readonly string[] ArabicTwos =
            {
                "مئتان", "ألفان", "مليونان", "ملياران", "تريليونان", "كوادريليونان", "كوينتليونان", "سكستيليونان"
            };

        /// <summary>
        ///     The english group.
        /// </summary>
        private static readonly string[] EnglishGroup =
            {
                "Hundred", "Thousand", "Million", "Billion", "Trillion", "Quadrillion", "Quintillion", "Sextillian",
                "Septillion", "Octillion", "Nonillion", "Decillion", "Undecillion", "Duodecillion", "Tredecillion",
                "Quattuordecillion", "Quindecillion", "Sexdecillion", "Septendecillion", "Octodecillion",
                "Novemdecillion", "Vigintillion", "Unvigintillion", "Duovigintillion", "10^72", "10^75", "10^78",
                "10^81", "10^84", "10^87", "Vigintinonillion", "10^93", "10^96", "Duotrigintillion", "Trestrigintillion"
            };

        /// <summary>
        ///     The english ones.
        /// </summary>
        private static readonly string[] EnglishOnes =
            {
                "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
                "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
            };

        /// <summary>
        ///     The english tens.
        /// </summary>
        private static readonly string[] EnglishTens =
            {
                "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
            };

        /// <summary>
        ///     Decimal Part
        /// </summary>
        private int decimalValue;

        /// Group Levels: 987,654,321.234
        /// 234 : Group Level -1
        /// 321 : Group Level 0
        /// 654 : Group Level 1
        /// 987 : Group Level 2
        /// <summary>
        ///     integer part
        /// </summary>
        private long intergerValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberToWord"/> class.
        ///     Constructor: short version
        /// </summary>
        /// <param name="number">
        /// Number to be converted
        /// </param>
        /// <param name="currency">
        /// Currency to use
        /// </param>
        /// <param name="addOnlyPart">
        /// The add Only Part.
        /// </param>
        public NumberToWord(decimal number, CurrencyInfo currency, bool addOnlyPart = true)
        {
            this.InitializeClass(
                number,
                currency,
                string.Empty,
                addOnlyPart ? "only." : string.Empty,
                addOnlyPart ? "فقط" : string.Empty,
                addOnlyPart ? "لا غير." : string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberToWord"/> class.
        ///     Constructor: Full Version
        /// </summary>
        /// <param name="number">
        /// Number to be converted
        /// </param>
        /// <param name="currency">
        /// Currency to use
        /// </param>
        /// <param name="englishPrefixText">
        /// English text to be placed before the generated text
        /// </param>
        /// <param name="englishSuffixText">
        /// English text to be placed after the generated text
        /// </param>
        /// <param name="arabicPrefixText">
        /// Arabic text to be placed before the generated text
        /// </param>
        /// <param name="arabicSuffixText">
        /// Arabic text to be placed after the generated text
        /// </param>
        public NumberToWord(
            decimal number,
            CurrencyInfo currency,
            string englishPrefixText,
            string englishSuffixText,
            string arabicPrefixText,
            string arabicSuffixText)
        {
            this.InitializeClass(
                number,
                currency,
                englishPrefixText,
                englishSuffixText,
                arabicPrefixText,
                arabicSuffixText);
        }

        /// <summary>
        ///     Arabic text to be placed before the generated text
        /// </summary>
        public string ArabicPrefixText { get; set; }

        /// <summary>
        ///     Arabic text to be placed after the generated text
        /// </summary>
        public string ArabicSuffixText { get; set; }

        /// <summary>
        ///     Currency to use
        /// </summary>
        public CurrencyInfo Currency { get; set; }

        /// <summary>
        ///     English text to be placed before the generated text
        /// </summary>
        public string EnglishPrefixText { get; set; }

        /// <summary>
        ///     English text to be placed after the generated text
        /// </summary>
        public string EnglishSuffixText { get; set; }

        /// <summary>
        ///     Number to be converted
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        ///     Convert stored number to words using selected currency
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string ConvertToArabic()
        {
            var tempNumber = this.Number;

            if (tempNumber == 0)
            {
                return "صفر";
            }

            // Get Text for the decimal part
            var decimalString = this.ProcessArabicGroup(this.decimalValue, -1, 0);

            var retVal = string.Empty;
            byte group = 0;
            while (tempNumber >= 1)
            {
                // seperate number into groups
                var numberToProcess = (int)(tempNumber % 1000);

                tempNumber = tempNumber / 1000;

                // convert group into its text
                var groupDescription = this.ProcessArabicGroup(numberToProcess, group, Math.Floor(tempNumber));

                if (groupDescription != string.Empty)
                {
                    // here we add the new converted group to the previous concatenated text
                    if (group > 0)
                    {
                        if (retVal != string.Empty)
                        {
                            retVal = $"و {retVal}";
                        }

                        if (numberToProcess != 2)
                        {
                            if (numberToProcess % 100 != 1)
                            {
                                if (numberToProcess >= 3 && numberToProcess <= 10)
                                {
                                    // for numbers between 3 and 9 we use plural name
                                    retVal = $"{ArabicPluralGroups[group]} {retVal}";
                                }
                                else
                                {
                                    retVal = retVal != string.Empty
                                                 ? $"{ArabicAppendedGroup[group]} {retVal}"
                                                 : $"{ArabicGroup[group]} {retVal}";
                                }
                            }
                            else
                            {
                                retVal = $"{ArabicGroup[group]} {retVal}"; // use normal case
                            }
                        }
                    }

                    retVal = $"{groupDescription} {retVal}";
                }

                group++;
            }

            var formattedNumber = string.Empty;
            formattedNumber += this.ArabicPrefixText != string.Empty ? $"{this.ArabicPrefixText} " : string.Empty;
            formattedNumber += retVal != string.Empty ? retVal : string.Empty;
            if (this.intergerValue != 0)
            {
                // here we add currency name depending on _intergerValue : 1 ,2 , 3--->10 , 11--->99
                var remaining100 = (int)(this.intergerValue % 100);

                if (remaining100 == 0)
                {
                    formattedNumber += this.Currency.Arabic1CurrencyName;
                }
                else if (remaining100 == 1)
                {
                    formattedNumber += this.Currency.Arabic1CurrencyName;
                }
                else if (remaining100 == 2)
                {
                    if (this.intergerValue == 2)
                    {
                        formattedNumber += this.Currency.Arabic2CurrencyName;
                    }
                    else
                    {
                        formattedNumber += this.Currency.Arabic1CurrencyName;
                    }
                }
                else if (remaining100 >= 3 && remaining100 <= 10)
                {
                    formattedNumber += this.Currency.Arabic310CurrencyName;
                }
                else if (remaining100 >= 11 && remaining100 <= 99)
                {
                    formattedNumber += this.Currency.Arabic1199CurrencyName;
                }
            }

            formattedNumber += this.decimalValue != 0 ? " و " : string.Empty;
            formattedNumber += this.decimalValue != 0 ? decimalString : string.Empty;
            if (this.decimalValue != 0)
            {
                // here we add currency part name depending on _intergerValue : 1 ,2 , 3--->10 , 11--->99
                formattedNumber += " ";

                var remaining100 = this.decimalValue % 100;

                if (remaining100 == 0)
                {
                    formattedNumber += this.Currency.Arabic1CurrencyPartName;
                }
                else if (remaining100 == 1)
                {
                    formattedNumber += this.Currency.Arabic1CurrencyPartName;
                }
                else if (remaining100 == 2)
                {
                    formattedNumber += this.Currency.Arabic2CurrencyPartName;
                }
                else if (remaining100 >= 3 && remaining100 <= 10)
                {
                    formattedNumber += this.Currency.Arabic310CurrencyPartName;
                }
                else if (remaining100 >= 11 && remaining100 <= 99)
                {
                    formattedNumber += this.Currency.Arabic1199CurrencyPartName;
                }
            }

            formattedNumber += this.ArabicSuffixText != string.Empty ? $" {this.ArabicSuffixText}" : string.Empty;

            return formattedNumber;
        }

        /// <summary>
        ///     Convert stored number to words using selected currency
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string ConvertToEnglish()
        {
            var tempNumber = this.Number;

            if (tempNumber == 0)
            {
                return "Zero";
            }

            var decimalString = this.ProcessGroup(this.decimalValue);

            var retVal = string.Empty;

            var group = 0;

            if (tempNumber < 1)
            {
                retVal = EnglishOnes[0];
            }
            else
            {
                while (tempNumber >= 1)
                {
                    var numberToProcess = (int)(tempNumber % 1000);

                    tempNumber = tempNumber / 1000;

                    var groupDescription = this.ProcessGroup(numberToProcess);

                    if (groupDescription != string.Empty)
                    {
                        if (group > 0)
                        {
                            retVal = $"{EnglishGroup[group]} {retVal}";
                        }

                        retVal = $"{groupDescription} {retVal}";
                    }

                    group++;
                }
            }

            var formattedNumber = string.Empty;
            formattedNumber += this.EnglishPrefixText != string.Empty ? $"{this.EnglishPrefixText} " : string.Empty;
            formattedNumber += retVal != string.Empty ? retVal : string.Empty;
            formattedNumber += retVal != string.Empty
                                   ? (this.intergerValue == 1
                                          ? this.Currency.EnglishCurrencyName
                                          : this.Currency.EnglishPluralCurrencyName)
                                   : string.Empty;
            formattedNumber += decimalString != string.Empty ? " and " : string.Empty;
            formattedNumber += decimalString != string.Empty ? decimalString : string.Empty;
            formattedNumber += decimalString != string.Empty
                                   ? " " + (this.decimalValue == 1
                                                ? this.Currency.EnglishCurrencyPartName
                                                : this.Currency.EnglishPluralCurrencyPartName)
                                   : string.Empty;
            formattedNumber += this.EnglishSuffixText != string.Empty ? $" {this.EnglishSuffixText}" : string.Empty;

            return formattedNumber;
        }

        /// <summary>
        ///     Eextract Interger and Decimal parts
        /// </summary>
        private void ExtractIntegerAndDecimalParts()
        {
            var splits = this.Number.ToString(CultureInfo.InvariantCulture).Split('.');

            this.intergerValue = Convert.ToInt32(splits[0]);

            if (splits.Length > 1)
            {
                this.decimalValue = Convert.ToInt32(this.GetDecimalValue(splits[1]));
            }
        }

        /// <summary>
        /// Get Proper Decimal Value
        /// </summary>
        /// <param name="decimalPart">
        /// Decimal Part as a String
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetDecimalValue(string decimalPart)
        {
            string result;

            if (this.Currency.PartPrecision != decimalPart.Length)
            {
                var decimalPartLength = decimalPart.Length;

                for (var i = 0; i < this.Currency.PartPrecision - decimalPartLength; i++)
                {
                    decimalPart += "0"; // Fix for 1 number after decimal ( 10.5 , 1442.2 , 375.4 )
                }

                result =
                    $"{decimalPart.Substring(0, this.Currency.PartPrecision)}.{decimalPart.Substring(this.Currency.PartPrecision, decimalPart.Length - this.Currency.PartPrecision)}";

                result = Math.Round(Convert.ToDecimal(result)).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                result = decimalPart;
            }

            for (var i = 0; i < this.Currency.PartPrecision - result.Length; i++)
            {
                result += "0";
            }

            return result;
        }

        /// <summary>
        /// Get Feminine Status of one digit
        /// </summary>
        /// <param name="digit">
        /// The Digit to check its Feminine status
        /// </param>
        /// <param name="groupLevel">
        /// Group Level
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetDigitFeminineStatus(int digit, int groupLevel)
        {
            if (groupLevel == -1)
            {
                // if it is in the decimal part
                if (this.Currency.IsCurrencyPartNameFeminine)
                {
                    return ArabicFeminineOnes[digit]; // use feminine field
                }

                return ArabicOnes[digit];
            }

            if (groupLevel == 0)
            {
                if (this.Currency.IsCurrencyNameFeminine)
                {
                    return ArabicFeminineOnes[digit]; // use feminine field
                }

                return ArabicOnes[digit];
            }

            return ArabicOnes[digit];
        }

        /// <summary>
        /// Initialize Class Varaibles
        /// </summary>
        /// <param name="number">
        /// Number to be converted
        /// </param>
        /// <param name="currency">
        /// Currency to use
        /// </param>
        /// <param name="englishPrefixText">
        /// English text to be placed before the generated text
        /// </param>
        /// <param name="englishSuffixText">
        /// English text to be placed after the generated text
        /// </param>
        /// <param name="arabicPrefixText">
        /// Arabic text to be placed before the generated text
        /// </param>
        /// <param name="arabicSuffixText">
        /// Arabic text to be placed after the generated text
        /// </param>
        private void InitializeClass(
            decimal number,
            CurrencyInfo currency,
            string englishPrefixText,
            string englishSuffixText,
            string arabicPrefixText,
            string arabicSuffixText)
        {
            this.Number = number;
            this.Currency = currency;
            this.EnglishPrefixText = englishPrefixText;
            this.EnglishSuffixText = englishSuffixText;
            this.ArabicPrefixText = arabicPrefixText;
            this.ArabicSuffixText = arabicSuffixText;

            this.ExtractIntegerAndDecimalParts();
        }

        /// <summary>
        /// Process a group of 3 digits
        /// </summary>
        /// <param name="groupNumber">
        /// The group number to process
        /// </param>
        /// <param name="groupLevel">
        /// </param>
        /// <param name="remainingNumber">
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ProcessArabicGroup(int groupNumber, int groupLevel, decimal remainingNumber)
        {
            var tens = groupNumber % 100;

            var hundreds = groupNumber / 100;

            var retVal = string.Empty;

            if (hundreds > 0)
            {
                if (tens == 0 && hundreds == 2)
                {
                    // حالة المضاف
                    retVal = $"{ArabicAppendedTwos[0]}";
                }
                else
                {
                    // الحالة العادية
                    retVal = $"{ArabicHundreds[hundreds]}";
                }
            }

            if (tens > 0)
            {
                if (tens < 20)
                {
                    // if we are processing under 20 numbers
                    if (tens == 2 && hundreds == 0 && groupLevel > 0)
                    {
                        // This is special case for number 2 when it comes alone in the group
                        if (this.intergerValue == 2000 || this.intergerValue == 2000000
                                                       || this.intergerValue == 2000000000
                                                       || this.intergerValue == 2000000000000
                                                       || this.intergerValue == 2000000000000000
                                                       || this.intergerValue == 2000000000000000000)
                        {
                            retVal = $"{ArabicAppendedTwos[groupLevel]}"; // في حالة الاضافة
                        }
                        else
                        {
                            retVal = $"{ArabicTwos[groupLevel]}"; // في حالة الافراد
                        }
                    }
                    else
                    {
                        // General case
                        if (retVal != string.Empty)
                        {
                            retVal += " و ";
                        }

                        if (tens == 1 && groupLevel > 0 && hundreds == 0)
                        {
                            retVal += " ";
                        }
                        else if ((tens == 1 || tens == 2) && (groupLevel == 0 || groupLevel == -1) && hundreds == 0
                                 && remainingNumber == 0)
                        {
                            retVal += string.Empty;

                            // Special case for 1 and 2 numbers like: ليرة سورية و ليرتان سوريتان
                        }
                        else
                        {
                            retVal += this.GetDigitFeminineStatus(tens, groupLevel);

                            // Get Feminine status for this digit
                        }
                    }
                }
                else
                {
                    var ones = tens % 10;
                    tens = tens / 10 - 2; // 20's offset

                    if (ones > 0)
                    {
                        if (retVal != string.Empty)
                        {
                            retVal += " و ";
                        }

                        // Get Feminine status for this digit
                        retVal += this.GetDigitFeminineStatus(ones, groupLevel);
                    }

                    if (retVal != string.Empty)
                    {
                        retVal += " و ";
                    }

                    // Get Tens text
                    retVal += ArabicTens[tens];
                }
            }

            return retVal;
        }

        /// <summary>
        /// Process a group of 3 digits
        /// </summary>
        /// <param name="groupNumber">
        /// The group number to process
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ProcessGroup(int groupNumber)
        {
            var tens = groupNumber % 100;

            var hundreds = groupNumber / 100;

            var retVal = string.Empty;

            if (hundreds > 0)
            {
                retVal = $"{EnglishOnes[hundreds]} {EnglishGroup[0]}";
            }

            if (tens > 0)
            {
                if (tens < 20)
                {
                    retVal += (retVal != string.Empty ? " " : string.Empty) + EnglishOnes[tens];
                }
                else
                {
                    var ones = tens % 10;

                    tens = tens / 10 - 2; // 20's offset

                    retVal += (retVal != string.Empty ? " " : string.Empty) + EnglishTens[tens];

                    if (ones > 0)
                    {
                        retVal += (retVal != string.Empty ? " " : string.Empty) + EnglishOnes[ones];
                    }
                }
            }

            return retVal;
        }
    }
}