using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

    // 외곽선(블랙)으로 칠 색상의 최대치. 숫자가 커질수록 애매하게 검은 색상도 완전한 검은색으로 취급된다. 
    [SerializeField]
    int outlineThreshold = 30;

    public int StageIndex { get; set; }

    public Material SdfMaterial => sdfMaterial;
    public TextAsset RawStageData => rawStageData;
    public Texture2D A1Tex => a1Tex;
    public Texture2D A2Tex => a2Tex;
    public int OutlineThreshold => outlineThreshold;

    public StageSequenceData StageSequenceData => Data.dataSet.StageSequenceData[StageIndex];

    // [NonSerialized] 속성을 안붙이면 cachedStageData가 null이 아니면서 내용물이 다 텅 빈 상태가 되기도 한다...
    // 왜지...?
    [NonSerialized]
    StageData cachedStageData;

    public StageData StageData => cachedStageData ??= LoadStageData();

    StageData LoadStageData()
    {
        using var stream = new MemoryStream(RawStageData.bytes);
        var formatter = new BinaryFormatter();
        var stageData = (StageData) formatter.Deserialize(stream);
        stream.Close();
        return stageData;
    }

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