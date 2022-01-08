using System;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

[DisallowMultipleComponent]
public class ProfilePopup : MonoBehaviour
{
    [SerializeField]
    GameObject stageImagePrefab;

    [SerializeField]
    Transform stageImageParent;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    [UsedImplicitly]
    async void OpenPopup()
    {
        stageImageParent.DestroyImmediateAllChildren();
        
        var pngFiles = Directory.GetFiles(Application.persistentDataPath, "*.png")
            .OrderBy(e => e, StringComparer.Ordinal);
        
        foreach (var pngFile in pngFiles)
        {
            Debug.Log(pngFile);

            var stageName = Path.GetFileNameWithoutExtension(pngFile);
            
            var stageId = int.Parse(stageName, NumberStyles.Any);

            var stageMetadata = await StageDetail.LoadStageMetadataByZeroBasedIndexAsync(stageId - 1);

            if (stageMetadata != null)
            {
                if (stageMetadata.StageIndex + 1 <= BlackContext.instance.LastClearedStageId)
                {
                    var stageButton = Instantiate(stageImagePrefab, stageImageParent).GetComponent<StageButton>();
                    stageButton.SetStageMetadata(stageMetadata);
                    stageButton.Unlock();
                }
            }
        }
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}