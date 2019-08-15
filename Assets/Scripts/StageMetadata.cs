using UnityEngine;

public class StageMetadata : ScriptableObject {
    [SerializeField] Sprite gridWorldSprite = null;
    [SerializeField] Material skipBlackMaterial = null;
    [SerializeField] Sprite sdfSprite = null;
    [SerializeField] Material sdfMaterial = null;
    [SerializeField] TextAsset islandData = null;
}
