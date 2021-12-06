using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

internal static class StageEditorUtil
{
    [MenuItem("Assets/Black/Create New Stage")]
    [UsedImplicitly]
    static void CreateNewStage()
    {
        foreach (var o in Selection.objects)
        {
            if (!(o is TextAsset ta)) continue;
            
            var metaFullPath = AssetDatabase.GetAssetPath(ta);
            var stageDir = Path.GetDirectoryName(metaFullPath);
            Debug.Log(stageDir);
            if (stageDir == null)
            {
                continue;
            }

            var metaFileNameWithoutExt = Path.GetFileNameWithoutExtension(metaFullPath);

            if (string.IsNullOrEmpty(metaFileNameWithoutExt))
            {
                continue;
            }

            var sdfTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(metaFullPath, metaFileNameWithoutExt,
                "-OTB-FSNB-BB-SDF.png"));
            var fsnbTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(metaFullPath,
                metaFileNameWithoutExt,
                "-OTB-FSNB.png"));

            var sdfPreset = AssetDatabase.LoadAssetAtPath<Preset>("Presets/TextureImporter-SDF");
            var fsnbPreset = AssetDatabase.LoadAssetAtPath<Preset>("Presets/TextureImporter-FSNB");

            if (sdfPreset == null || fsnbPreset == null) continue;

            if (sdfTex == null || fsnbTex == null) continue;
            
            Debug.Log(ta);
            
            sdfPreset.ApplyTo(sdfTex);
            EditorUtility.SetDirty(sdfTex);
            
            fsnbPreset.ApplyTo(fsnbTex);
            EditorUtility.SetDirty(fsnbTex);
        }
    }
}