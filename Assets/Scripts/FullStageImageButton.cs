using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class FullStageImageButton : MonoBehaviour
{
    [SerializeField]
    IslandShader3DController islandShader3DController;

    [SerializeField]
    StageDetailPopup stageDetailPopupForReplay;

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
        
        await stageDetailPopupForReplay.OpenPopupAfterLoadingAsync(stageIndex);
    }

    public void Initialize(StageMetadata stageMetadata, StageDetailPopup inStageDetailPopupForReplay)
    {
        stageDetailPopupForReplay = inStageDetailPopupForReplay;
        islandShader3DController.Initialize(stageMetadata);
    }
}