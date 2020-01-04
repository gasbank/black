using UnityEngine;
using UnityEditor;

public class StageSpriteAssetPostprocessor : AssetPostprocessor {
    void OnPostProcessSprites(Texture2D texture, Sprite[] sprites) {
        Debug.Log($"Postprocessing Sprites: {assetPath}...");
        if (assetPath.EndsWith("-FSNB.png")) {
            Debug.Log($"Got it Sprite: {assetPath}");
        }
    }

    void OnPostprocessTexture(Texture2D texture) {
        Debug.Log($"Postprocessing Texture: {assetPath}...");
        if (assetPath.EndsWith("-FSNB.png")) {
            var importer = assetImporter as TextureImporter;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var s = new TextureImporterPlatformSettings();
            s.format = TextureImporterFormat.RGB24;
            s.name = "Default";
            importer.SetPlatformTextureSettings(s);
            //importer.textureFormat = TextureImporterFormat.RGB24;
            importer.isReadable = true;
            Debug.Log($"Got it Texture: {assetPath}");
            //texture.filterMode = FilterMode.Point;
            texture.filterMode = FilterMode.Point;
        }
    }
}
