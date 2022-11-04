using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class LocalizedTextUtil
{
    static readonly int petaPostfixLength = ",000,000,000,000,000".Length;
    static readonly int teraPostfixLength = ",000,000,000,000".Length;
    static readonly int gigaPostfixLength = ",000,000,000".Length;
    static readonly int megaPostfixLength = ",000,000".Length;
    static readonly int kiloPostfixLength = ",000".Length;

    public static readonly string[] koreanCurrencyPostfixList =
    {
        null, null, null, null, "만", null, null, null, "억", null, null, null, "조", null, null, null, "경", null, null,
        null, "해", null, null, null, "자", null, null, null, "양", null, null, null, "구", null, null, null, "간", null
    };

    public static readonly string[] japaneseCurrencyPostfixList =
    {
        null, null, null, null, "万", null, null, null, "億", null, null, null, "兆", null, null, null, "京", null, null,
        null, "垓", null, null, null, "じょ", null, null, null, "穣", null, null, null, "溝", null, null, null, "澗", null
    };

    public static readonly string[] chineseCurrencyPostfixList =
    {
        null, null, null, null, "万", null, null, null, "亿", null, null, null, "兆", null, null, null, "京", null, null,
        null, "垓", null, null, null, "秭", null, null, null, "A", null, null, null, "B", null, null, null, "C", null
    };

    public static readonly string[] taiwanCurrencyPostfixList =
    {
        null, null, null, null, "萬", null, null, null, "億", null, null, null, "兆", null, null, null, "京", null, null,
        null, "垓", null, null, null, "秭", null, null, null, "A", null, null, null, "B", null, null, null, "C", null
    };

    public static readonly string[] englishCurrencyPostfixList =
    {
        null, null, null, "k", null, null, "m", null, null, "g", null, null, "t", null, null, "p", null, null, "e",
        null, null, "z", null, null, "y", null
    };

    public static readonly Dictionary<BlackLanguageCode, string[]> currencyPostfixListDict =
        new Dictionary<BlackLanguageCode, string[]>
        {
            {BlackLanguageCode.Ko, koreanCurrencyPostfixList},
            {BlackLanguageCode.Ja, japaneseCurrencyPostfixList},
            {BlackLanguageCode.Ch, chineseCurrencyPostfixList},
            {BlackLanguageCode.Tw, taiwanCurrencyPostfixList},
            {BlackLanguageCode.En, englishCurrencyPostfixList}
        };

    public static readonly CultureInfo DefaultCultureInfo = new CultureInfo("en-US");

    public static string Localized(this string str)
    {
        return FontManager.Instance != null ? FontManager.Instance.ToLocalizedCurrent(str) : str;
    }

    public static string Localized(this string str, params object[] args)
    {
        return FontManager.Instance != null
            ? FontManager.Instance.ToLocalizedCurrent(str, args)
            : string.Format(str, args);
    }

    public static string Localized(this ScString str)
    {
        return FontManager.Instance != null ? FontManager.Instance.ToLocalizedCurrent(str) : str.ToString();
    }

    public static string Localized(this ScString str, params object[] args)
    {
        return FontManager.Instance != null
            ? FontManager.Instance.ToLocalizedCurrent(str, args)
            : string.Format(str.ToString(), args);
    }

    public static string Postfixed(this IFormattable formattable)
    {
        return formattable.ToString("n0", DefaultCultureInfo);
    }

    static string PostfixedForCurrency(IFormattable bigInteger, string[] currencyPostfixList)
    {
        var s = bigInteger.ToString();
        var sLen = s.Length;
        var sb = new StringBuilder();
        var omitZero = false;
        var postfixAppended = false;
        var postfixCount = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] != '0' || omitZero == false)
            {
                if (postfixAppended)
                {
                    sb.Append(' ');
                    postfixAppended = false;
                }

                sb.Append(s[i]);
                omitZero = false;
            }

            var irev = s.Length - i - 1;
            var postfixStr =
                currencyPostfixList[irev < currencyPostfixList.Length ? irev : currencyPostfixList.Length - 1];
            if (postfixStr != null && omitZero == false)
            {
                sb.Append(postfixStr);
                omitZero = true;
                postfixCount++;
                if (postfixCount >= 3) break;
                postfixAppended = true;
            }
        }

        return sb.ToString();
    }
}