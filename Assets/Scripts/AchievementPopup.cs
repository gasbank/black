using System;
using System.Collections.Generic;
using System.Linq;
using ConditionalDebug;
using Dirichlet.Numerics;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AchievementPopup : MonoBehaviour
{
    public static AchievementPopup instance;

    // 표시 순서 때문에 순서를 정할 목적으로 만든 배열
    static readonly string[] groupKeyArray =
    {
        "lastClearedStageId"
    };

    public ScrollRect achievementScrollView;
    public RectTransform achievementScrollViewRect;
    public Image achievementTabImage;
    public Text achievementTabText;
    public Color activeTabColor;
    public Sprite activeTabSprite;
    public ScrollRect bigPictureScrollView;
    public RectTransform bigPictureScrollViewRect;
    public Image bigPictureTabImage;
    public Text bigPictureTabText;

    [SerializeField]
    Animator bigPopupAnimator;

    [SerializeField]
    CanvasGroup canvasGroup;

    [SerializeField]
    GameObjectToggle gameObjectToggle;

    public Color inactiveTabColor;
    public Sprite inactiveTabSprite;
    int lastTabIndex;
    public Color normalColor;
    public Color redeemedColor;

#if UNITY_EDITOR
    void OnValidate()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
#endif

    void OnOpenPopup()
    {
        bigPictureTabImage.gameObject.SetActive(true);

        OpenAchievementTab();

        if (BlackContext.instance.IsBigPopupOpened == false) bigPopupAnimator.Play("Big Popup Appear", -1, 0);

        BlackContext.instance.OpenBigPopup(canvasGroup);
        BackButtonHandler.instance.PushAction(gameObjectToggle.Toggle);
    }

    void OnClosePopup()
    {
        StopScrolling();
        BlackContext.instance.CloseBigPopup(canvasGroup);
        BackButtonHandler.instance.PopAction();
    }

    // 스크롤뷰에 관성이 있기 때문에 창이 닫힌 후에도
    // 멈춰추지 않으면 계속 스크롤된다.
    // (이는 관성 스크롤 상태로 만들고 팝업을 빠르게 닫았다 열었을 때도 확인 가능하다.)
    // 부자연스러우므로 멈춰주자.
    void StopScrolling()
    {
        achievementScrollView.velocity = Vector2.zero;
        bigPictureScrollView.velocity = Vector2.zero;
    }

    public void Close()
    {
        gameObjectToggle.Toggle();
    }

    public void OpenAchievementTab()
    {
        if (Data.dataSet == null) return;

        achievementTabImage.sprite = activeTabSprite;
        achievementTabText.color = activeTabColor;
        bigPictureTabImage.sprite = inactiveTabSprite;
        bigPictureTabText.color = inactiveTabColor;

        achievementScrollViewRect.gameObject.SetActive(true);
        bigPictureScrollViewRect.gameObject.SetActive(false);

        UpdateAchievementTab();
        lastTabIndex = 0;
    }

    public void UpdateAchievementTab(string updateGroupKey = "")
    {
        if (lastTabIndex != 0) return;

        if (BlackContext.instance == null) return;

        if (Data.dataSet == null) return;

        // short names
        var gathered = BlackContext.instance.AchievementGathered;
        var redeemed = BlackContext.instance.AchievementRedeemed;
        var group = Data.achievementOrderedGroup;

        if (gathered == null || redeemed == null) return;

        var canBeRedeemedAchievements = new List<Tuple<AchievementData, UInt128>>();

        var addCanBeRedeemedAchievementFuncMap = new Dictionary<string, Action>
        {
            {
                "lastClearedStageId",
                () => AddCanBeRedeemedAchievement(canBeRedeemedAchievements, group, "lastClearedStageId",
                    gathered.MaxBlackLevel, redeemed.MaxBlackLevel)
            }
        };

        // 퀘스트 창이 열려 있지 않고, 특정 퀘스트만 업데이트 되었다는 정보가 있다면
        // 이에 대해서만 체크하고 돌아가면 된다.
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (canvasGroup.alpha == 0 && string.IsNullOrEmpty(updateGroupKey) == false)
        {
            //ConDebug.Log("UpdateAchievementTab() [FAST FUNCTION]");

            addCanBeRedeemedAchievementFuncMap[updateGroupKey]();

            // 켜져 있는 것을 끌 수는 없다. 안켜져있을 때만 켜질 가능성이 있다.
            if (BlackContext.instance.AchievementNewImage.activeSelf == false)
                BlackContext.instance.AchievementNewImage.SetActive(
                    canBeRedeemedAchievements.Count > 0);

            // 그리고 이 다음부터는 아무것도 할 필요 없다. 창이 안열려있는걸~~
            return;
        }

        // 모든 퀘스트 그룹에 대해 보상 받을 수 있는 것을 체크한다.
        foreach (var groupKey in groupKeyArray) addCanBeRedeemedAchievementFuncMap[groupKey]();

        // 빨간색 아이콘 상태 갱신
        BlackContext.instance.AchievementNewImage.SetActive(canBeRedeemedAchievements.Count > 0);

        // 모든 업적에 대해서 체크는 했지만, 역시 창이 안열려있으면 이 이후는 처리가 필요없다.
        if (canvasGroup.alpha == 0)
        {
            ConDebug.Log("UpdateAchievementTab() [MID FUNCTION]");
            return;
        }

        ConDebug.Log("UpdateAchievementTab() [SLOW FUNCTION]");


        var ongoingAchievements = new List<Tuple<AchievementData, UInt128>>();
        AddOngoingAchievement(ongoingAchievements, group, "lastClearedStageId", gathered.MaxBlackLevel);

        var totalCount = canBeRedeemedAchievements.Count + ongoingAchievements.Count;
        var entries = achievementScrollViewRect.GetComponentsInChildren<AchievementEntry>(true);
        if (entries.Length < totalCount)
        {
            for (var i = 0; i < totalCount - entries.Length; i++)
                InstantiateLocalized.InstantiateLocalize(entries[0].gameObject, entries[0].transform.parent);

            // Refresh 'entries'
            entries = achievementScrollViewRect.GetComponentsInChildren<AchievementEntry>(true);
        }

        var count = Mathf.Min(totalCount, entries.Length);
        // Adjust scroll view total height
        var entryIndex = 0;
        for (var i = 0; i < canBeRedeemedAchievements.Count; i++)
        {
            var ach = canBeRedeemedAchievements[i].Item1;
            var currentValue = canBeRedeemedAchievements[i].Item2;
            entries[entryIndex].gameObject.SetActive(true);
            entries[entryIndex].image.sprite = Atlas.Load(ach.sprite);
            entries[entryIndex].image.color = Color.white;
            entries[entryIndex].achievementName.text = ach.name.Localized(ach.conditionOldArg.ToLong().Postfixed(),
                ach.conditionNewArg.ToLong().Postfixed());
            entries[entryIndex].achievementDesc.text = ach.desc.Localized(ach.conditionOldArg.ToLong().Postfixed(),
                ach.conditionNewArg.ToLong().Postfixed(), currentValue.Postfixed());
            entries[entryIndex].redeemButton.gameObject.SetActive(true);
            entries[entryIndex].redeemButton.interactable = true;
            entries[entryIndex].rewardGemText.text = ach.RewardGemString;
            entries[entryIndex].achievementData = ach;
            entries[entryIndex].rewardGemText.gameObject.SetActive(false);
            entries[entryIndex].rewardGemText.gameObject.SetActive(true);
            entryIndex++;
        }

        for (var i = 0; i < ongoingAchievements.Count; i++)
        {
            var ach = ongoingAchievements[i].Item1;
            var currentValue = ongoingAchievements[i].Item2;
            entries[entryIndex].gameObject.SetActive(true);
            entries[entryIndex].image.sprite = Atlas.Load(ach.sprite);
            entries[entryIndex].image.color = Color.white;

            entries[entryIndex].achievementName.text = ach.name.Localized(ach.conditionOldArg.ToLong().Postfixed(),
                ach.conditionNewArg.ToLong().Postfixed());
            entries[entryIndex].achievementDesc.text = ach.desc.Localized(ach.conditionOldArg.ToLong().Postfixed(),
                ach.conditionNewArg.ToLong().Postfixed(), currentValue.Postfixed());
            entries[entryIndex].redeemButton.gameObject.SetActive(true);
            entries[entryIndex].redeemButton.interactable = false;
            entries[entryIndex].rewardGemText.text = ach.RewardGemString;
            entries[entryIndex].achievementData = ach;
            entries[entryIndex].rewardGemText.gameObject.SetActive(false);
            entries[entryIndex].rewardGemText.gameObject.SetActive(true);
            entryIndex++;
        }

        for (var i = Mathf.Max(0, count); i < entries.Length; i++) entries[i].gameObject.SetActive(false);
    }

    public void OnNewImage()
    {
        if (!BlackContext.instance.AchievementNewImage.activeSelf)
            BlackContext.instance.AchievementNewImage.SetActive(true);
    }

    static void AddCanBeRedeemedAchievement(List<Tuple<AchievementData, UInt128>> canBeRedeemedAchievements,
        Dictionary<ScString, List<AchievementData>> group, string groupKey, UInt128 gatheredValue,
        UInt128 redeemedValue)
    {
        var gatheredValueClamped = gatheredValue.ToClampedLong();
        //Debug.Log($"groupKey={groupKey}");
        var achievementGroup = group[groupKey];
        var gatheredIndex = Data.AchievementData_ConditionNewArg_UpperBound(achievementGroup, gatheredValueClamped) - 1;

        var redeemedValueClamped = redeemedValue.ToClampedLong();
        var nextRedeemIndex = Data.AchievementData_ConditionNewArg_UpperBound(achievementGroup, redeemedValueClamped);

        if (nextRedeemIndex <= gatheredIndex && nextRedeemIndex < achievementGroup.Count)
            canBeRedeemedAchievements.Add(
                new Tuple<AchievementData, UInt128>(achievementGroup[nextRedeemIndex], gatheredValue));
    }

    static void AddOngoingAchievement(List<Tuple<AchievementData, UInt128>> ongoingAchievements,
        Dictionary<ScString, List<AchievementData>> group, string groupKey, UInt128 gatheredValue)
    {
        var ongoingAchievementData = group[groupKey]
            .FirstOrDefault(x => x.conditionOldArg <= gatheredValue && gatheredValue < x.conditionNewArg);
        if (ongoingAchievementData != null)
            ongoingAchievements.Add(new Tuple<AchievementData, UInt128>(ongoingAchievementData, gatheredValue));
    }
}