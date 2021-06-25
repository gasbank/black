using UnityEngine;
using UnityEngine.Serialization;

public class StageMetadata : ScriptableObject {
    [SerializeField]
    Material skipBlackMaterial;

    [SerializeField]
    Material sdfMaterial;

    [SerializeField]
    TextAsset rawStageData;

    [SerializeField]
    string friendlyStageName = "Stage Name";

    public Material SkipBlackMaterial => skipBlackMaterial;
    public Material SdfMaterial => sdfMaterial;
    public TextAsset RawStageData => rawStageData;
    public string FriendlyStageName => friendlyStageName;
}