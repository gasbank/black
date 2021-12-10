using System;
using UnityEngine;
using UnityEngine.U2D;

public class Atlas {
    public static Sprite Load(string path) {
        Sprite sprite = null;

        if (path.StartsWith("Sprites/Icons/")) {
            var atlasName = "Resources Icon + UI (Lucky Draw)";
            sprite = LoadAtlasSprite(path, atlasName);
        } else if (path.StartsWith("Sprites/su/")) {
            var atlasName = "Resources Black (Whole)";
            sprite = LoadAtlasSprite(path, atlasName);
        } else if (path.StartsWith("Sprites/su_parts/")) {
            var atlasName = "Resources Black";
            sprite = LoadAtlasSprite(path, atlasName);
        }

        return sprite;
    }

    static Sprite LoadAtlasSprite(string path, string atlasName) {
        var spriteName = path.Substring(path.LastIndexOf("/", StringComparison.Ordinal) + 1);

//#if UNITY_EDITOR
//        var atlasPath = $"Assets/Resources/Atlas/{atlasName}";
//        var spriteAtlas = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/Resources/Atlas/{atlasName}");
//#else
        var atlasPath = $"Atlas/{atlasName}";
        var spriteAtlas = Resources.Load<SpriteAtlas>(atlasPath);
//#endif

        if (spriteAtlas != null) {
            return spriteAtlas.GetSprite(spriteName);
        }

        Debug.LogError($"Atlas.LoadAtlasSprite: Cannot find sprite name '{spriteName}' from atlas path '{atlasPath}'. Atlas is null!");
        return null;
    }

#if UNITY_EDITOR
    public static Sprite LoadEditor(string path) {
        var assetPath = $"Assets/{path}.png";
        var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null) {
            Debug.LogError($"Atlas.LoadEditor: Cannot find sprite path '{assetPath}'.");
        }
        return sprite;
    }
#endif
}
