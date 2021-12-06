using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

internal static class StageEditorUtil
{
    static readonly int SdfTexture = Shader.PropertyToID("SdfTexture");
    static readonly int ColorTexture = Shader.PropertyToID("ColorTexture");

    [MenuItem("Assets/Black/Create New Stage")]
    [UsedImplicitly]
    static void CreateNewStage()
    {
        foreach (var o in Selection.objects)
        {
            if (!(o is TextAsset rawStageData)) continue;

            var rawStageDataFullPath = AssetDatabase.GetAssetPath(rawStageData);
            var stageDir = Path.GetDirectoryName(rawStageDataFullPath);
            Debug.Log(stageDir);
            if (string.IsNullOrEmpty(stageDir))
            {
                Debug.LogError("Stage directory is empty.");
                continue;
            }

            var stageName = Path.GetFileNameWithoutExtension(stageDir);
            if (string.IsNullOrEmpty(stageName))
            {
                Debug.LogError("Stage name cannot be determined.");
                continue;
            }

            var metadataName = Path.GetFileNameWithoutExtension(rawStageDataFullPath);
            if (string.IsNullOrEmpty(metadataName))
            {
                Debug.LogError("Metadata name cannot be determined.");
                continue;
            }

            var sdfTexPath = Path.Combine(stageDir, $"{metadataName}-OTB-FSNB-BB-SDF.png");
            var sdfTex = AssetDatabase.LoadAssetAtPath<Texture2D>(sdfTexPath);

            var fsnbTexPath = Path.Combine(stageDir, $"{metadataName}-OTB-FSNB.png");
            var fsnbTex = AssetDatabase.LoadAssetAtPath<Texture2D>(fsnbTexPath);

            var sdfPresetPath = "Assets/Presets/TextureImporter-SDF.preset";

            var sdfPreset = AssetDatabase.LoadAssetAtPath<Preset>(sdfPresetPath);

            var fsnbPresetPath = "Assets/Presets/TextureImporter-FSNB.preset";
            var fsnbPreset = AssetDatabase.LoadAssetAtPath<Preset>(fsnbPresetPath);

            if (sdfPreset == null)
            {
                Debug.LogError($"SDF Preset not found on path '{sdfPresetPath}'");
                continue;
            }

            if (fsnbPreset == null)
            {
                Debug.LogError($"FSNB Preset not found on path '{fsnbPresetPath}'");
                continue;
            }

            if (sdfTex == null)
            {
                Debug.LogError($"SDF Texture not found on path '{sdfTexPath}'");
                continue;
            }

            if (fsnbTex == null)
            {
                Debug.LogError($"FSNB Texture not found on path '{fsnbTexPath}'");
                continue;
            }

            Debug.Log(rawStageData);

            var sdfImporter = AssetImporter.GetAtPath(sdfTexPath);
            Debug.Log(sdfTex);
            Debug.Log(sdfPreset.ApplyTo(sdfImporter));

            var fsnbImporter = AssetImporter.GetAtPath(fsnbTexPath);
            Debug.Log(fsnbTex);
            Debug.Log(fsnbPreset.ApplyTo(fsnbImporter));

            var skipBlackMatPresetPath = "Assets/Presets/Material-SkipBlack.preset";
            var skipBlackMatPreset = AssetDatabase.LoadAssetAtPath<Preset>(skipBlackMatPresetPath);

            var skipBlackMat = new Material(Shader.Find("Specular"));
            skipBlackMatPreset.ApplyTo(skipBlackMat);
            skipBlackMat.SetTexture(ColorTexture, fsnbTex);
            EditorUtility.SetDirty(skipBlackMat);
            AssetDatabase.CreateAsset(skipBlackMat, Path.Combine(stageDir, $"{stageName}.mat"));

            var sdfMatPresetPath = "Assets/Presets/Material-SDF.preset";
            var sdfBlackMatPreset = AssetDatabase.LoadAssetAtPath<Preset>(sdfMatPresetPath);

            var sdfMat = new Material(Shader.Find("Specular"));
            sdfBlackMatPreset.ApplyTo(sdfMat);
            sdfMat.SetTexture(SdfTexture, sdfTex);
            EditorUtility.SetDirty(sdfMat);
            AssetDatabase.CreateAsset(sdfMat, Path.Combine(stageDir, $"{stageName}-SDF.mat"));

            var stageMetadataPath = Path.Combine(stageDir, $"{stageName}.asset");
            var stageMetadata = StageMetadata.Create(skipBlackMat, sdfMat, rawStageData, stageName);
            AssetDatabase.CreateAsset(stageMetadata, stageMetadataPath);

            Debug.Log($"{stageName} stage created.");
        }
    }
}