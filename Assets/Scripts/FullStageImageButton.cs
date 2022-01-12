using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class FullStageImageButton : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public async void OnClick()
    {
        Sound.instance.PlayButtonClick();
        
        var stageMetadata = await StageDetail.LoadStageMetadataByZeroBasedIndexAsync(transform.GetSiblingIndex());
        if (stageMetadata == null)
        {
            Debug.LogError("StageMetadata null");
            return;
        }

        var stageTitle = Data.dataSet.StageSequenceData[stageMetadata.StageIndex].title;

        ConfirmPopup.instance.OpenYesNoPopup(@"\'{0}' 스테이지를 다시 시작할까요?\n\n설정 메뉴에서 언제든지 미술관으로 돌아올 수 있습니다.".Localized(stageTitle),
            () =>
            {
                StageButton.SetCurrentStageMetadataForce(stageMetadata);
                SaveLoadManager.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance, null);
                SceneManager.LoadScene("Main");
            }, ConfirmPopup.instance.Close);
    }
}