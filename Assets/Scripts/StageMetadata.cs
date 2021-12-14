using UnityEngine;

public class StageMetadata : ScriptableObject
{
    [SerializeField]
    Material skipBlackMaterial;

    [SerializeField]
    Material sdfMaterial;

    [SerializeField]
    TextAsset rawStageData;

    [SerializeField]
    int starCount;

    [SerializeField]
    string friendlyStageName = "Stage Name";

    public static StageMetadata Create(Material skipBlackMat, Material sdfMat, TextAsset stageData, string stageName)
    {
        var asset = CreateInstance<StageMetadata>();
        asset.skipBlackMaterial = skipBlackMat;
        asset.sdfMaterial = sdfMat;
        asset.rawStageData = stageData;
        asset.friendlyStageName = stageName;
        return asset;
    }

    public Material SkipBlackMaterial => skipBlackMaterial;
    public Material SdfMaterial => sdfMaterial;
    public TextAsset RawStageData => rawStageData;
    public string FriendlyStageName => friendlyStageName;
    public int StarCount => starCount;
}