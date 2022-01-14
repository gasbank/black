using System;
using ConditionalDebug;
using UnityEngine;

public class EaselButton : MonoBehaviour
{
    [SerializeField]
    StageDetail stageDetail;

    [SerializeField]
    MuseumImage museumImage;

    [SerializeField]
    EaselExclamationMark exclamationMark;

    void OnEnable()
    {
        BlackContext.instance.OnGoldChanged += OnGoldChanged;
    }

    void OnDisable()
    {
        BlackContext.instance.OnGoldChanged -= OnGoldChanged;
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
                                             StageDetail.IsAllCleared == false);
    }

    public async void OnClick()
    {
        Sound.instance.PlayButtonClick();

        var lastClearedStageId = BlackContext.instance.LastClearedStageId;
        ConDebug.Log($"Last Cleared Stage ID: {lastClearedStageId}");
        
        await stageDetail.OpenPopupAfterLoadingAsync(lastClearedStageId);
    }
}