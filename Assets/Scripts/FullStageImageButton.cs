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

        var stageIndex = transform.GetSiblingIndex();

        await StageDetail.instance.OpenPopupAfterLoadingAsync(stageIndex);
    }

    public async void OnClickXXX()
    {
        var stageMetadata = await StageDetail.LoadStageMetadataByZeroBasedIndexAsync(transform.GetSiblingIndex());
        if (stageMetadata == null)
        {
            Debug.LogError("StageMetadata null");
            return;
        }

        var stageTitle = Data.dataSet.StageSequenceData[stageMetadata.StageIndex].title;

        
    }
}