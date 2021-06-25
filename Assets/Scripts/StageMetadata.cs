using UnityEngine;

public class StageMetadata : ScriptableObject {
    [SerializeField] Material skipBlackMaterial;
    [SerializeField] Material sdfMaterial;
    [SerializeField] TextAsset rawStageData;

    public Material SkipBlackMaterial => skipBlackMaterial;
    public Material SdfMaterial => sdfMaterial;
    public TextAsset RawStageData => rawStageData;
}
