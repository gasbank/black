using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class ProfilePopup : MonoBehaviour
{
    [SerializeField]
    GameObject stageImagePrefab;

    [SerializeField]
    Transform stageImageParent;

    [SerializeField]
    int cachedLastClearedStageId;

    [SerializeField]
    GameObject fullStageImagePrefab;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    [UsedImplicitly]
    async void OpenPopup()
    {
        if (cachedLastClearedStageId == BlackContext.instance.LastClearedStageId)
        {
            return;
        }
        
        stageImageParent.DestroyImmediateAllChildren();

        for (var i = 1; i <= BlackContext.instance.LastClearedStageId; i++)
        {
            var islandShader3DController = Instantiate(fullStageImagePrefab, stageImageParent)
                .GetComponent<IslandShader3DController>();

            var stageMetadata = await StageDetail.LoadStageMetadataByZeroBasedIndexAsync(i - 1);

            if (stageMetadata == null)
            {
                Debug.LogError($"Stage metadata not found for zero based index {i - 1}");
            }
            
            islandShader3DController.Initialize(stageMetadata);
        }

        cachedLastClearedStageId = BlackContext.instance.LastClearedStageId;
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}