using System;
using ConditionalDebug;
using UnityEngine;
using UnityEngine.Serialization;

public class EaselButton : MonoBehaviour
{
    [FormerlySerializedAs("stageDetail")]
    [SerializeField]
    StageDetailPopup stageDetailPopup;

    [SerializeField]
    MuseumImage museumImage;

    [SerializeField]
    EaselExclamationMark exclamationMark;

    void OnEnable()
    {
        BlackContext.Instance.OnGoldChanged += OnGoldChanged;
    }

    void OnDisable()
    {
        BlackContext.Instance.OnGoldChanged -= OnGoldChanged;
    }

    void Start()
    {
        UpdateExclamationMark();
    }

    void OnGoldChanged()
    {
        UpdateExclamationMark();
    }

    void UpdateExclamationMark()
    {
        exclamationMark.gameObject.SetActive(museumImage.IsAnyExclamationMarkShown == false &&
                                             StageDetailPopup.IsAllCleared == false);
    }

    public async void OnClick()
    {
        Sound.Instance.PlayButtonClick();

        var lastClearedStageId = BlackContext.Instance.LastClearedStageId;
        ConDebug.Log($"Last Cleared Stage ID: {lastClearedStageId}");
        
        await stageDetailPopup.OpenPopupAfterLoadingAsync(lastClearedStageId);
    }
}