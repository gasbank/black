using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MuseumLevelGroup : MonoBehaviour
{
    public const string Level0Text = "---";
    public const string Level1Text = "1";
    public const string Level0Message = "미술관 꾸미기는 현재 개발 중입니다!";
    public const string LegacyLevel0Message = "미술관 레벨입니다. 본격적인 재건을 시작하기 전까지는 사용할 수 없습니다.";
    public const string Level1Message = "미술관 개관을 축하드립니다. 미술관이 제 모습을 되찾아나가면서 레벨이 증가하게 됩니다";

    [SerializeField, AutoBind]
    Text levelText;

    bool? lastLevel1State;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void OnEnable()
    {
        Refresh();
    }

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        var isLevel1 = IsMuseumLevel1Unlocked();
        if (lastLevel1State != isLevel1)
        {
            Refresh(isLevel1);
        }
    }

    public static string ResolveMessage(string message)
    {
        // Existing scenes still pass the level 0 help text directly through button bindings.
        return IsMuseumLevel1Unlocked() && IsMuseumLevelMessage(message) ? Level1Message : message;
    }

    static bool IsMuseumLevelMessage(string message)
    {
        return message == Level0Message || message == LegacyLevel0Message;
    }

    public static bool IsMuseumLevel1Unlocked()
    {
        return BlackContext.Instance != null &&
               BlackContext.Instance.LoadedAtLeastOnce &&
               BlackContext.Instance.HasPlayedMuseumLevel1Transition;
    }

    void Refresh()
    {
        Refresh(IsMuseumLevel1Unlocked());
    }

    void Refresh(bool isLevel1)
    {
        if (levelText == null)
        {
            levelText = transform.Find("Level Text")?.GetComponent<Text>();
        }

        lastLevel1State = isLevel1;

        if (levelText != null)
        {
            levelText.text = isLevel1 ? Level1Text : Level0Text;
        }
    }
}
