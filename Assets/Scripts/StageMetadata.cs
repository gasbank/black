using UnityEngine;

public class StageMetadata : ScriptableObject
{
    [SerializeField]
    TextAsset rawStageData;

    [SerializeField]
    Material sdfMaterial;

    [SerializeField]
    Texture2D a1Tex;

    [SerializeField]
    Texture2D a2Tex;

    public int StageIndex { get; set; }

    public Material SdfMaterial => sdfMaterial;
    public TextAsset RawStageData => rawStageData;
    public Texture2D A1Tex => a1Tex;
    public Texture2D A2Tex => a2Tex;

    public StageSequenceData StageSequenceData => Data.dataSet.StageSequenceData[StageIndex];

#if UNITY_EDITOR
    public static StageMetadata Create()
    {
        return CreateInstance<StageMetadata>();
    }

    public static void SetValues(StageMetadata asset, Material sdfMat, TextAsset stageData,
        Texture2D a1Tex, Texture2D a2Tex)
    {
        asset.sdfMaterial = sdfMat;
        asset.rawStageData = stageData;
        asset.a1Tex = a1Tex;
        asset.a2Tex = a2Tex;
    }
#endif
}