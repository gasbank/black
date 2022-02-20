using UnityEngine;

#if ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

public class StageButtonGroup : MonoBehaviour
{
    [SerializeField]
    GameObject stageButtonPrefab;

#if ADDRESSABLES
    async void Start()
    {
        var stageAssetLocList = await Addressables.LoadResourceLocationsAsync("Stage", typeof(StageMetadata)).Task;
        foreach (var loc in stageAssetLocList)
        {
            var stageMetadata = await Addressables.LoadAssetAsync<StageMetadata>(loc).Task;
            var stageButton = Instantiate(stageButtonPrefab, transform).GetComponent<StageButton>();
            stageButton.SetStageMetadata(stageMetadata);
            stageButton.gameObject.name = stageMetadata.name;
        }


//        var stageAssetList = await Addressables.LoadAssetsAsync<StageMetadata>("Stage", null).Task;
//        foreach (var stageMetadata in stageAssetList)
//        {
//            var stageButton = Instantiate(stageButtonPrefab, transform).GetComponent<StageButton>();
//            stageButton.SetStageMetadata(stageMetadata);
//            stageButton.gameObject.name = stageMetadata.name;
//            await Task.Delay(1);
//        }
    }
#endif
}