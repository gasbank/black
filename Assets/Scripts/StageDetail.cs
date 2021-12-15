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

    public void OpenPopup()
    {
        var lastClearedStageId = BlackContext.instance.LastClearedStageId;
        Debug.Log($"Last Cleared Stage ID: {lastClearedStageId}");

        if (lastClearedStageId < 0) lastClearedStageId = 0;

        if (lastClearedStageId >= Data.dataSet.StageMetadataList.Count)
        {
            //Debug.LogError("lastClearedStageId exceeds Data.dataSet.StageMetadataList count.");
            ConfirmPopup.instance.Open(@"\모든 스테이지를 깼습니다!", ConfirmPopup.instance.Close);
            return;
        }

        var stageMetadataLoc = Data.dataSet.StageMetadataList[lastClearedStageId];
        if (stageMetadataLoc == null)
        {
            Debug.LogError($"Stage metadata at index {lastClearedStageId} is null");
            return;
        }

        stageProgress.ProgressInt = lastClearedStageId % 5;
        
        Addressables.LoadAssetAsync<StageMetadata>(stageMetadataLoc).Completed += stageMetadataHandle =>
        {
            stageButton.SetStageMetadata(stageMetadataHandle.Result);
            subcanvas.Open();
        };
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }

    public void OnStageStartButton()
    {
        stageButton.SetStageMetadataToCurrent();
        SceneManager.LoadScene("Main");
    }
}