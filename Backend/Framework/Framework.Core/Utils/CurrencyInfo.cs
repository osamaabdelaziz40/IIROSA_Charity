// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrencyInfo.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Utils
{
    #region usings

    using System;

    #endregion usings

    /// <summary>
    ///     The currency info.
    /// </summary>
    public class CurrencyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyInfo"/> class.
        /// </summary>
        /// <param name="currency">
        /// The currency.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public CurrencyInfo(Currencies currency)
        {
            switch (currency)
            {
                case Currencies.GenericNumber:

                    this.CurrencyId = 0;
                    this.CurrencyCode = string.Empty;
                    this.IsCurrencyNameFeminine = true;
                    this.EnglishCurrencyName = string.Empty;
                    this.EnglishPluralCurrencyName = string.Empty;
                    this.EnglishCurrencyPartName = string.Empty;
                    this.EnglishPluralCurrencyPartName = string.Empty;
                    this.Arabic1CurrencyName = string.Empty;
                    this.Arabic2CurrencyName = string.Empty;
                    this.Arabic310CurrencyName = string.Empty;
                    this.Arabic1199CurrencyName = string.Empty;
                    this.Arabic1CurrencyPartName = string.Empty;
                    this.Arabic2CurrencyPartName = string.Empty;
                    this.Arabic310CurrencyPartName = string.Empty;
                    this.Arabic1199CurrencyPartName = string.Empty;
                    this.PartPrecision = 1;
                    this.IsCurrencyPartNameFeminine = true;
                    break;

                case Currencies.Syria:
                    this.CurrencyId = 0;
                    this.CurrencyCode = "SYP";
                    this.IsCurrencyNameFeminine = true;
                    this.EnglishCurrencyName = "Syrian Pound";
                    this.EnglishPluralCurrencyName = "Syrian Pounds";
                    this.EnglishCurrencyPartName = "Piaster";
                    this.EnglishPluralCurrencyPartName = "Piasteres";
                    this.Arabic1CurrencyName = "ليرة سورية";
                    this.Arabic2CurrencyName = "ليرتان سوريتان";
                    this.Arabic310CurrencyName = "ليرات سورية";
                    this.Arabic1199CurrencyName = "ليرة سورية";
                    this.Arabic1CurrencyPartName = "قرش";
                    this.Arabic2CurrencyPartName = "قرشان";
                    this.Arabic310CurrencyPartName = "قروش";
                    this.Arabic1199CurrencyPartName = "قرشاً";
                    this.PartPrecision = 2;
                    this.IsCurrencyPartNameFeminine = false;
                    break;

                case Currencies.Uae:
                    this.CurrencyId = 1;
                    this.CurrencyCode = "AED";
                    this.IsCurrencyNameFeminine = false;
                    this.EnglishCurrencyName = "UAE Dirham";
                    this.EnglishPluralCurrencyName = "UAE Dirhams";
                    this.EnglishCurrencyPartName = "Fils";
                    this.EnglishPluralCurrencyPartName = "Fils";
                    this.Arabic1CurrencyName = "درهم إماراتي";
                    this.Arabic2CurrencyName = "درهمان إماراتيان";
                    this.Arabic310CurrencyName = "دراهم إماراتية";
                    this.Arabic1199CurrencyName = "درهماً إماراتياً";
                    this.Arabic1CurrencyPartName = "فلس";
                    this.Arabic2CurrencyPartName = "فلسان";
                    this.Arabic310CurrencyPartName = "فلوس";
                    this.Arabic1199CurrencyPartName = "فلساً";
                    this.PartPrecision = 2;
                    this.IsCurrencyPartNameFeminine = false;
                    break;

                case Currencies.SaudiArabia:
                    this.CurrencyId = 2;
                    this.CurrencyCode = "SAR";
                    this.IsCurrencyNameFeminine = false;
                    this.EnglishCurrencyName = "Saudi Riyal";
                    this.EnglishPluralCurrencyName = "Saudi Riyals";
                    this.EnglishCurrencyPartName = "Halala";
                    this.EnglishPluralCurrencyPartName = "Halalas";
                    this.Arabic1CurrencyName = "ريال سعودي";
                    this.Arabic2CurrencyName = "ريالان سعوديان";
                    this.Arabic310CurrencyName = "ريالات سعودية";
                    this.Arabic1199CurrencyName = "ريالاً سعودياً";
                    this.Arabic1CurrencyPartName = "هللة";
                    this.Arabic2CurrencyPartName = "هللتان";
                    this.Arabic310CurrencyPartName = "هللات";
                    this.Arabic1199CurrencyPartName = "هللة";
                    this.PartPrecision = 2;
                    this.IsCurrencyPartNameFeminine = true;
                    break;

                case Currencies.Tunisia:
                    this.CurrencyId = 3;
                    this.CurrencyCode = "TND";
                    this.IsCurrencyNameFeminine = false;
                    this.EnglishCurrencyName = "Tunisian Dinar";
                    this.EnglishPluralCurrencyName = "Tunisian Dinars";
                    this.EnglishCurrencyPartName = "milim";
                    this.EnglishPluralCurrencyPartName = "millimes";
                    this.Arabic1CurrencyName = "دينار تونسي";
                    this.Arabic2CurrencyName = "ديناران تونسيان";
                    this.Arabic310CurrencyName = "دنانير تونسية";
                    this.Arabic1199CurrencyName = "ديناراً تونسياً";
                    this.Arabic1CurrencyPartName = "مليم";
                    this.Arabic2CurrencyPartName = "مليمان";
                    this.Arabic310CurrencyPartName = "ملاليم";
                    this.Arabic1199CurrencyPartName = "مليماً";
                    this.PartPrecision = 3;
                    this.IsCurrencyPartNameFeminine = false;
                    break;

                case Currencies.Gold:
                    this.CurrencyId = 4;
                    this.CurrencyCode = "XAU";
                    this.IsCurrencyNameFeminine = false;
                    this.EnglishCurrencyName = "Gram";
                    this.EnglishPluralCurrencyName = "Grams";
                    this.EnglishCurrencyPartName = "Milligram";
                    this.EnglishPluralCurrencyPartName = "Milligrams";
                    this.Arabic1CurrencyName = "جرام";
                    this.Arabic2CurrencyName = "جرامان";
                    this.Arabic310CurrencyName = "جرامات";
                    this.Arabic1199CurrencyName = "جراماً";
                    this.Arabic1CurrencyPartName = "ملجرام";
                    this.Arabic2CurrencyPartName = "ملجرامان";
                    this.Arabic310CurrencyPartName = "ملجرامات";
                    this.Arabic1199CurrencyPartName = "ملجراماً";
                    this.PartPrecision = 2;
                    this.IsCurrencyPartNameFeminine = false;
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(currency), currency, null);
            }
        }

        /// <summary>
        ///     The currencies.
        /// </summary>
        public enum Currencies
        {
            /// <summary>
            ///     The syria.
            /// </summary>
            Syria = 0,

            /// <summary>
            ///     The uae.
            /// </summary>
            Uae,

            /// <summary>
            ///     The saudi arabia.
            /// </summary>
            SaudiArabia,

            /// <summary>
            ///     The tunisia.
            /// </summary>
            Tunisia,

            /// <summary>
            ///     The gold.
            /// </summary>
            Gold,

            /// <summary>
            ///     The generic number.
            /// </summary>
            GenericNumber
        }

        /// <summary>
        ///     Arabic Currency Name for 11 to 99 units
        ///     خمس و سبعون ليرةً سوريةً
        ///     خمسة و سبعون درهماً إماراتياً
        /// </summary>
        public string Arabic1199CurrencyName { get; set; }

        /// <summary>
        ///     Arabic Currency Part Name for 11 to 99 units
        ///     قرشاً
        ///     هللةً
        /// </summary>
        public string Arabic1199CurrencyPartName { get; set; }

        /// <summary>
        ///     Arabic Currency Name for 1 unit only
        ///     ليرة سورية
        ///     درهم إماراتي
        /// </summary>
        public string Arabic1CurrencyName { get; set; }

        /// <summary>
        ///     Arabic Currency Part Name for 1 unit only
        ///     قرش
        ///     هللة
        /// </summary>
        public string Arabic1CurrencyPartName { get; set; }

        /// <summary>
        ///     Arabic Currency Name for 2 units only
        ///     ليرتان سوريتان
        ///     درهمان إماراتيان
        /// </summary>
        public string Arabic2CurrencyName { get; set; }

        /// <summary>
        ///     Arabic Currency Part Name for 2 unit only
        ///     قرشان
        ///     هللتان
        /// </summary>
        public string Arabic2CurrencyPartName { get; set; }

        /// <summary>
        ///     Arabic Currency Name for 3 to 10 units
        ///     خمس ليرات سورية
        ///     خمسة دراهم إماراتية
        /// </summary>
        public string Arabic310CurrencyName { get; set; }

        /// <summary>
        ///     Arabic Currency Part Name for 3 to 10 units
        ///     قروش
        ///     هللات
        /// </summary>
        public string Arabic310CurrencyPartName { get; set; }

        /// <summary>
        ///     Standard Code
        ///     Syrian Pound: SYP
        ///     UAE Dirham: AED
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        ///     Currency ID
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        ///     English Currency Name for single use
        ///     Syrian Pound
        ///     UAE Dirham
        /// </summary>
        public string EnglishCurrencyName { get; set; }

        /// <summary>
        ///     English Currency Part Name for single use
        ///     Piaster
        ///     Fils
        /// </summary>
        public string EnglishCurrencyPartName { get; set; }

        /// <summary>
        ///     English Plural Currency Name for Numbers over 1
        ///     Syrian Pounds
        ///     UAE Dirhams
        /// </summary>
        public string EnglishPluralCurrencyName { get; set; }

        /// <summary>
        ///     English Currency Part Name for Plural
        ///     Piasters
        ///     Fils
        /// </summary>
        public string EnglishPluralCurrencyPartName { get; set; }

        /// <summary>
        ///     Is the currency name feminine ( Mua'anath مؤنث)
        ///     ليرة سورية : مؤنث = true
        ///     درهم : مذكر = false
        /// </summary>
        public bool IsCurrencyNameFeminine { get; set; }

        /// <summary>
        ///     Is the currency part name feminine ( Mua'anath مؤنث)
        ///     هللة : مؤنث = true
        ///     قرش : مذكر = false
        /// </summary>
        public bool IsCurrencyPartNameFeminine { get; set; }

        /// <summary>
        ///     Decimal Part Precision
        ///     for Syrian Pounds: 2 ( 1 SP = 100 parts)
        ///     for Tunisian Dinars: 3 ( 1 TND = 1000 parts)
        /// </summary>
        public byte PartPrecision { get; set; }
    }
}