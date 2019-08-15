using UnityEngine;
using UnityEditor;

public class StageMetadataEditor {
    [MenuItem("Assets/Create/Stage Metadata")]
    public static void CreateAssetMenu() {
        var asset = ScriptableObject.CreateInstance<StageMetadata>();
        AssetDatabase.CreateAsset(asset, "Assets/Resources/StageMetadata.asset");
		EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
