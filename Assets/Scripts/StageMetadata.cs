using UnityEngine;

public class StageMetadata : ScriptableObject
{
    [SerializeField]
    string friendlyStageName = "Stage Name";

    [SerializeField]
    TextAsset rawStageData;

    [SerializeField]
    Material sdfMaterial;

    [SerializeField]
    Material skipBlackMaterial;

    [SerializeField]
    int starCount;

    [SerializeField]
    float remainTime;

    public Material SkipBlackMaterial => skipBlackMaterial;
    public Material SdfMaterial => sdfMaterial;
    public TextAsset RawStageData => rawStageData;
    public string FriendlyStageName => friendlyStageName;
    public int StarCount => starCount;
    public float RemainTime => remainTime;

    public static StageMetadata Create(Material skipBlackMat, Material sdfMat, TextAsset stageData, string stageName)
    {
        var asset = CreateInstance<StageMetadata>();
        asset.skipBlackMaterial = skipBlackMat;
        asset.sdfMaterial = sdfMat;
        asset.rawStageData = stageData;
        asset.friendlyStageName = stageName;
        return asset;
    }
}