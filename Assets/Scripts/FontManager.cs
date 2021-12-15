using System;
using System.Collections.Generic;
using ConditionalDebug;
using UnityEngine;
using Object = UnityEngine.Object;

public class FontManager : MonoBehaviour
{
    public static FontManager instance;

    static readonly BlackCustomFormatter blackCustomFormatter = new BlackCustomFormatter();

    [SerializeField]
    Font englishFont;

    [SerializeField]
    Font japaneseFont;

    [SerializeField]
    Font koreanFont;

    [SerializeField]
    Font simplifiedChineseFont;

    [SerializeField]
    Font systemFont;

    [SerializeField]
    Font traditionalChineseFont;

    public Font SystemFont => systemFont;

    bool ParseStrRef(string strRef, out string key, out int index)
    {
        key = "";
        index = -1;
        if (strRef.Length < 2) return false;

        if (strRef.StartsWith("\\"))
        {
            var sharpIndex = strRef.LastIndexOf("#", StringComparison.Ordinal);
            if (sharpIndex >= 2 && sharpIndex < strRef.Length - 1)
            {
                key = strRef.Substring(1, sharpIndex - 1);
                if (int.TryParse(strRef.Substring(sharpIndex + 1), out index)) return true;
            }
            else
            {
                // # 뒤의 인덱스가 없으면 #1로 간주
                key = strRef.Substring(1, strRef.Length - 1);
                index = 1;
                return true;
            }
        }

        return false;
    }

    public string ToLocalizedCurrent(string strRef)
    {
        return ToLocalized(strRef, Data.instance.CurrentLanguageCode, null);
    }

    public string ToLocalizedCurrent(Object context, string strRef)
    {
        return ToLocalized(strRef, Data.instance.CurrentLanguageCode, context);
    }

    public string ToLocalizedCurrent(string strRef, params object[] args)
    {
        return ToLocalized(strRef, Data.instance.CurrentLanguageCode, null, args);
    }

    string ToLocalized(string strRef, BlackLanguageCode languageCode, Object context, params object[] args)
    {
        var ret = strRef;
        if (ParseStrRef(strRef, out var key, out var index))
            switch (languageCode)
            {
                case BlackLanguageCode.Tw:
                    ret = LookupLocalizedDictionary(key, index, Data.dataSet.StrTwData, strRef, languageCode, context);
                    break;
                case BlackLanguageCode.Ch:
                    ret = LookupLocalizedDictionary(key, index, Data.dataSet.StrChData, strRef, languageCode, context);
                    break;
                case BlackLanguageCode.Ja:
                    ret = LookupLocalizedDictionary(key, index, Data.dataSet.StrJaData, strRef, languageCode, context);
                    break;
                case BlackLanguageCode.En:
                    ret = LookupLocalizedDictionary(key, index, Data.dataSet.StrEnData, strRef, languageCode, context);
                    break;
                default:
                    ret = LookupLocalizedDictionary(key, index, Data.dataSet.StrKoData, strRef, languageCode, context);
                    break;
            }
        else
            Debug.LogWarningFormat("Localized string ref cannot be parsed: {0}", strRef);

        if (args != null && args.Length > 0)
            try
            {
                return string.Format(blackCustomFormatter, ret, args);
            }
            catch (FormatException)
            {
                Debug.LogError($"String Reference '{strRef}' (Value='{ret}') has format error!");
                return strRef;
            }

        return ret;
    }

    static string LookupLocalizedDictionary<T>(string key, int index, Dictionary<ScString, T> strBaseData,
        string strRef,
        BlackLanguageCode languageCode, Object context) where T : StrBaseData
    {
        if (strBaseData == null)
        {
            ConDebug.LogError("LookupLocalizedDictionary strBaseData null!", context);
        }
        else
        {
            if (strBaseData.TryGetValue(key, out var strData))
            {
                if (index - 1 >= 0 && index - 1 < strData.str.Length)
                {
                    if (strData.str[index - 1] != null) return ((string) strData.str[index - 1]).Replace(@"\n", "\n");

                    ConDebug.LogError(
                        $"Invalid(null) localized string index: key:{key} index:{index} on language code {languageCode}.",
                        context);
                }
                else
                {
                    ConDebug.LogError(
                        $"Invalid(out of range) localized string index: key:{key} index:{index} on language code {languageCode}.",
                        context);
                }
            }
            else
            {
                ConDebug.LogWarning(
                    $"Invalid(not found) localized string key: key:{key}/index:{index} on language code {languageCode}",
                    context);
            }
        }

        return strRef;
    }

    public Font GetLanguageFont(BlackLanguageCode languageCode)
    {
        switch (languageCode)
        {
            case BlackLanguageCode.Ko:
                return koreanFont;
            case BlackLanguageCode.Ja:
                return japaneseFont;
            case BlackLanguageCode.Ch:
                return simplifiedChineseFont;
            case BlackLanguageCode.Tw:
                return traditionalChineseFont;
            case BlackLanguageCode.En:
                return englishFont;
            default:
                return koreanFont;
        }
    }
}