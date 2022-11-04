using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class ProfilePopup : MonoBehaviour
{
    [SerializeField]
    GameObject stageImagePrefab;

    [SerializeField]
    Transform stageImageParent;

    [SerializeField]
    int cachedLastClearedStageId;

    [FormerlySerializedAs("fullStageImagePrefab")]
    [SerializeField]
    GameObject fullStageImageButtonPrefab;

    [SerializeField]
    StageDetailPopup stageDetailPopupForReplay;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    [UsedImplicitly]
    async void OpenPopup()
    {
        if (cachedLastClearedStageId == BlackContext.Instance.LastClearedStageId)
        {
            return;
        }
        
        stageImageParent.DestroyImmediateAllChildren();

        for (var i = 1; i <= BlackContext.Instance.LastClearedStageId; i++)
        {
            var fullStageImageButton = Instantiate(fullStageImageButtonPrefab, stageImageParent)
                .GetComponent<FullStageImageButton>();

            var stageMetadata = await StageDetailPopup.LoadStageMetadataByZeroBasedIndexAsync(i - 1);

            if (stageMetadata == null)
            {
                Debug.LogError($"Stage metadata not found for zero based index {i - 1}");
            }
            
            fullStageImageButton.Initialize(stageMetadata, stageDetailPopupForReplay);
        }

        cachedLastClearedStageId = BlackContext.Instance.LastClearedStageId;
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}