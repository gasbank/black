using UnityEngine;

public class StageMetadata : ScriptableObject {
    [SerializeField] Material skipBlackMaterial = null;
    [SerializeField] Material sdfMaterial = null;
    [SerializeField] TextAsset rawStageData = null;

    public Material SkipBlackMaterial => skipBlackMaterial;
    public Material SdfMaterial => sdfMaterial;
    public TextAsset RawStageData => rawStageData;
}
