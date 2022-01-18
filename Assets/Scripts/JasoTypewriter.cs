using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class JasoTypewriter : MonoBehaviour
{
    [SerializeField]
    Text targetText;

    Text TargetText => targetText;

    Coroutine coro;
    string FinalTargetText { get; set; }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    static readonly char[] CharList1 =
        {'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'};

    static readonly char[] CharList2 =
    {
        '\0', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ',
        'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'
    };

    string Separate(string str)
    {
        var cnt = str.Length;
        var chars = new List<char>();
        for (int i = 0; i < cnt; i++)
        {
            var cCode = (int) str[i];

            // 공백인 경우 바로 넣기
            if (cCode == 32)
            {
                chars.Add(' ');
                continue;
            }

            // 한글이 아닌 경우 한 글자 그대로 넣기
            if (cCode < 0xAC00 || cCode > 0xD7A3)
            {
                chars.Add(str[i]);
                continue;
            }

            cCode = str[i] - 0xAC00;
            var jong = cCode % 28;
            var jung = ((cCode - jong) / 28) % 21;
            var cho = (((cCode - jong) / 28) - jung) / 21;

            chars.Add(CharList1[cho]);
            chars.AddRange(char.ConvertFromUtf32(44032 + (cho * 588) + (jung * 28)));
            if (CharList2[jong] != '\0')
            {
                chars.AddRange(char.ConvertFromUtf32(44032 + (cho * 588) + (jung * 28) + jong));
            }
        }

        return new string(chars.ToArray());
    }

    IEnumerator TypeCoro(string finalText, bool isFastforward, Action onTypingFinish)
    {
        var separatedList = new List<string>();

        foreach (var f in finalText)
        {
            separatedList.Add(Separate(f.ToString()));
        }

        if (Time.timeScale <= 1)
        {
            var text = "";
            foreach (var separated in separatedList)
            {
                foreach (var ch in separated)
                {
                    targetText.text = text + ch;
                    if (isFastforward == false)
                    {
                        yield return new WaitForSeconds(1 / 30.0f);
                    }
                }

                text += separated[separated.Length - 1];
            }
        }

        targetText.text = finalText;
        coro = null;

        onTypingFinish?.Invoke();
    }

    void Awake()
    {
        UpdateFinalTargetText();
    }

    public void StartType(bool isFastforward, string text, Action onTypingFinish)
    {
        if (coro != null)
        {
            StopCoroutine(coro);
        }

        TargetText.gameObject.SetActive(true);

        FinalTargetText = text;

        coro = StartCoroutine(TypeCoro(FinalTargetText, isFastforward, onTypingFinish));
    }

    void UpdateFinalTargetText()
    {
        FinalTargetText = targetText.text;
    }

    public void ClearText()
    {
        targetText.text = string.Empty;
        UpdateFinalTargetText();
    }
}