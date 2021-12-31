using System;
using System.Threading.Tasks;
using ConditionalDebug;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageDetail : MonoBehaviour
{
    [SerializeField]
    StageButton stageButton;

    [SerializeField]
    Subcanvas subcanvas;

    [SerializeField]
    StageProgress stageProgress;

    [SerializeField]
    GameObject easelExclamationMark;

    [SerializeField]
    BottomTip bottomTip;

    [SerializeField]
    Subcanvas stageLockerSubcanvas;

    [SerializeField]
    Image stageLockerGauge;
    
    [SerializeField]
    Text stageLockerText;
    
    [SerializeField]
    float stageLockInitialTime;
    
    [SerializeField]
    float stageLockRemainTime;
    
    public static bool IsAllCleared => BlackContext.instance.LastClearedStageId >= Data.dataSet.StageSequenceData.Count;

    void Start()
    {
        SetInitialBottomTip();
    }

    public async void OpenPopupAfterLoadingAsync()
    {
        var lastClearedStageId = BlackContext.instance.LastClearedStageId;
        ConDebug.Log($"Last Cleared Stage ID: {lastClearedStageId}");

        if (lastClearedStageId < 0) lastClearedStageId = 0;

        if (IsAllCleared)
        {
            //Debug.LogError("lastClearedStageId exceeds Data.dataSet.StageMetadataList count.");
            ConfirmPopup.instance.Open(@"\모든 스테이지를 깼습니다!", ConfirmPopup.instance.Close);
            return;
        }

        var stageMetadataLoc = Data.dataSet.StageMetadataLocList[lastClearedStageId];
        if (stageMetadataLoc == null)
        {
            Debug.LogError($"Stage metadata at index {lastClearedStageId} is null");
            return;
        }

        stageProgress.ProgressInt = lastClearedStageId % 5;

        ProgressMessage.instance.Open("Loading...");

        var stageMetadata = await Addressables.LoadAssetAsync<StageMetadata>(stageMetadataLoc).Task;

        ProgressMessage.instance.Close();
        stageButton.SetStageMetadata(stageMetadata);
        subcanvas.Open();

        if (easelExclamationMark != null)
        {
            easelExclamationMark.SetActive(false);
        }

        if (bottomTip != null)
        {
            if (BlackContext.instance.LastClearedStageId == 0)
            {
                bottomTip.SetMessage("\\시작하기를 눌러 색칠을 시작하세요~!".Localized());
                bottomTip.OpenSubcanvas();
            }
            else if (BlackContext.instance.LastClearedStageId == 1)
            {
                bottomTip.SetMessage("\\잘 했어요! 다음 스테이지도 어서어서 색칠합시다.".Localized());
                bottomTip.OpenSubcanvas();
            }
            else if (BlackContext.instance.LastClearedStageId == 4)
            {
                bottomTip.SetMessage("\\이번 스테이지는 시간제한이 있는 '관문 스테이지'예요! 파이팅!!!".Localized());
                bottomTip.OpenSubcanvas();
            }
        }
    }

    [UsedImplicitly]
    public void OpenPopup()
    {
    }

    [UsedImplicitly]
    void ClosePopup()
    {
        if (easelExclamationMark != null)
        {
            easelExclamationMark.SetActive(IsAllCleared == false);
        }

        SetInitialBottomTip();
    }

    void SetInitialBottomTip()
    {
        if (bottomTip == null) return;
        
        if (BlackContext.instance.LastClearedStageId == 0)
        {
            bottomTip.SetMessage("\\이젤을 터치해서 색칠할 그림을 확인해봐요.".Localized());
        }
        else
        {
            bottomTip.CloseSubcanvas();
        }
    }

    public void OnStageStartButton()
    {
        Sound.instance.PlayButtonClick();
        stageButton.SetStageMetadataToCurrent();
        SaveLoadManager.instance.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance);
        SceneManager.LoadScene("Main");
    }

    void Update()
    {
        if (stageLockInitialTime <= 0 || stageLockRemainTime <= 0)
        {
            stageLockerSubcanvas.Close();
        }
        else
        {
            stageLockerSubcanvas.Open();
            stageLockRemainTime = Mathf.Max(0, stageLockRemainTime - Time.deltaTime);
            stageLockerGauge.fillAmount = stageLockRemainTime / stageLockInitialTime;
            stageLockerText.text = @"\다음 스테이지 준비중\n{0:F1}초".Localized(stageLockRemainTime);
        }
    }
}