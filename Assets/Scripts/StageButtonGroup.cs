using UnityEngine;
using UnityEngine.AddressableAssets;

public class StageButtonGroup : MonoBehaviour
{
    [SerializeField]
    GameObject stageButtonPrefab;
    
    async void Start()
    {
        var stageAssetList = await Addressables.LoadAssetsAsync<StageMetadata>("Stage", null).Task;
        foreach (var stageMetadata in stageAssetList)
        {
            var stageButton = Instantiate(stageButtonPrefab, transform).GetComponent<StageButton>();
            stageButton.SetStageMetadata(stageMetadata);
            stageButton.gameObject.name = stageMetadata.name;
        }
    }
}
