namespace IIROSA.Application.Services;

/// <summary>
/// Arabic amount-in-words conversion تفقيط (UC-CHQ-07) — turns a cheque amount into the
/// Arabic written form printed on the cheque face, e.g.
/// «فقط مائة وخمسة وعشرون جنيهاً وخمسون قرشاً لا غير».
/// Handles up to billions, two-decimal fractions, and the agreement rules of the counted
/// currency (one / dual / plural / singular-tanween).
/// </summary>
public static class ArabicAmountInWords
{
    private static readonly string[] Ones =
    {
        "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة"
    };

    private static readonly string[] Teens =
    {
        "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر",
        "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
    };

    private static readonly string[] Tens =
    {
        "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"
    };

    private static readonly string[] Hundreds =
    {
        "", "مائة", "مائتان", "ثلاثمائة", "أربعمائة", "خمسمائة",
        "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة"
    };

    /// <summary>Scale forms per count class: [one, dual, plural, singular-tanween, singular].</summary>
    private static readonly string[][] Scales =
    {
        // The units group has no scale word; placeholder keeps indexing simple.
        new[] { "", "", "", "", "" },
        new[] { "ألف", "ألفان", "آلاف", "ألفاً", "ألف" },
        new[] { "مليون", "مليونان", "ملايين", "مليوناً", "مليون" },
        new[] { "مليار", "ملياران", "مليارات", "ملياراً", "مليار" }
    };

    /// <summary>The counted-noun forms of one currency: main unit and its fraction (قرش/هللة/…).</summary>
    private readonly record struct CurrencyForms(
        string One, string Dual, string Plural, string Tanween, string Singular,
        string FracOne, string FracDual, string FracPlural, string FracTanween);

    private static readonly Dictionary<string, CurrencyForms> Currencies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["EGP"] = new("جنيه", "جنيهان", "جنيهات", "جنيهاً", "جنيه", "قرش", "قرشان", "قروش", "قرشاً"),
            ["SAR"] = new("ريال", "ريالان", "ريالات", "ريالاً", "ريال", "هللة", "هللتان", "هللات", "هللةً"),
            ["USD"] = new("دولار", "دولاران", "دولارات", "دولاراً", "دولار", "سنت", "سنتان", "سنتات", "سنتاً"),
            ["AED"] = new("درهم", "درهمان", "دراهم", "درهماً", "درهم", "فلس", "فلسان", "فلوس", "فلساً"),
            ["EUR"] = new("يورو", "يوروان", "يوروات", "يورو", "يورو", "سنت", "سنتان", "سنتات", "سنتاً"),
            ["YER"] = new("ريال", "ريالان", "ريالات", "ريالاً", "ريال", "فلس", "فلسان", "فلوس", "فلساً")
        };

    /// <summary>
    /// Converts an amount to its full Arabic cheque wording, wrapped in the customary
    /// «فقط … لا غير» frame.
    /// </summary>
    public static string Convert(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "A cheque amount cannot be negative.");
        }

        amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        var integerPart = (long)Math.Truncate(amount);
        var fractionPart = (int)((amount - Math.Truncate(amount)) * 100m);

        var unit = Currencies.TryGetValue(currency ?? string.Empty, out var u)
            ? u
            : new CurrencyForms("وحدة", "وحدتان", "وحدات", "وحدةً", "وحدة", "جزء", "جزءان", "أجزاء", "جزءاً");

        var parts = new List<string>();

        var integerWords = integerPart == 0
            ? "صفر"
            : NumberToWords(integerPart);
        parts.Add(JoinNumberWithUnit(integerWords, integerPart, unit.One, unit.Dual, unit.Plural, unit.Tanween, unit.Singular));

        if (fractionPart > 0)
        {
            var fractionWords = NumberToWords(fractionPart);
            parts.Add(JoinNumberWithUnit(fractionWords, fractionPart, unit.FracOne, unit.FracDual, unit.FracPlural, unit.FracTanween, unit.FracOne));
        }

        return $"فقط {string.Join(" و", parts)} لا غير";
    }

    /// <summary>
    /// Chooses the unit form that agrees with the count:
    /// 1 → «جنيه واحد», 2 → «جنيهان», 3–10 → plural, 11–99 → singular tanween, 100+ → singular.
    /// </summary>
    private static string JoinNumberWithUnit(
        string numberWords, long value,
        string one, string dual, string plural, string tanween, string singular)
    {
        if (value == 0)
        {
            return $"{numberWords} {singular}";
        }
        if (value == 1)
        {
            return $"{one} واحد";
        }
        if (value == 2)
        {
            return dual;
        }
        if (value is >= 3 and <= 10)
        {
            return $"{numberWords} {plural}";
        }
        if (value is >= 11 and <= 99)
        {
            return $"{numberWords} {tanween}";
        }
        return $"{numberWords} {singular}";
    }

    /// <summary>
    /// Converts a non-negative integer below 10^15 to Arabic words, group by group
    /// (billions → millions → thousands → units), joined with «و».
    /// </summary>
    private static string NumberToWords(long value)
    {
        if (value == 0)
        {
            return "صفر";
        }

        var groups = new List<int>();
        while (value > 0)
        {
            groups.Add((int)(value % 1000));
            value /= 1000;
        }

        var parts = new List<string>();
        for (var i = groups.Count - 1; i >= 0; i--)
        {
            var group = groups[i];
            if (group == 0)
            {
                continue;
            }

            var scale = Scales[i];

            if (i == 0)
            {
                parts.Add(GroupToWords(group));
            }
            else
            {
                parts.Add(group switch
                {
                    1 => scale[0],                       // ألف
                    2 => scale[1],                       // ألفان
                    >= 3 and <= 10 => $"{GroupToWords(group)} {scale[2]}",   // خمسة آلاف
                    >= 11 and <= 99 => $"{GroupToWords(group)} {scale[3]}",  // خمسة وعشرون ألفاً
                    _ => $"{GroupToWords(group)} {scale[4]}"                 // مائة ألف
                });
            }
        }

        return string.Join(" و", parts);
    }

    /// <summary>
    /// Converts a three-digit group (1–999) to Arabic words.
    /// </summary>
    private static string GroupToWords(int group)
    {
        var parts = new List<string>();

        var hundreds = group / 100;
        var remainder = group % 100;

        if (hundreds > 0)
        {
            parts.Add(Hundreds[hundreds]);
        }

        if (remainder > 0)
        {
            if (remainder < 10)
            {
                parts.Add(Ones[remainder]);
            }
            else if (remainder < 20)
            {
                parts.Add(Teens[remainder - 10]);
            }
            else
            {
                var units = remainder % 10;
                var tens = remainder / 10;
                parts.Add(units > 0 ? $"{Ones[units]} و{Tens[tens]}" : Tens[tens]);
            }
        }

        return string.Join(" و", parts);
    }
}
