using System;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using black_dev_tools;
using Object = UnityEngine.Object;

internal static class StageEditorUtil
{
    static readonly int SdfTexture = Shader.PropertyToID("SdfTexture");
    static readonly int ColorTexture = Shader.PropertyToID("ColorTexture");

    [MenuItem("Assets/Black/Import New Stage (PNG, JPEG Image)")]
    [UsedImplicitly]
    static void ImportNewStage_PngJpegImage()
    {
        if (Selection.objects != null)
        {
            try
            {
                for (var i = 0; i < Selection.objects.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Importing", Selection.objects[i].ToString(),
                        (float) i / Selection.objects.Length);

                    ImportNewStage_SingleSelected(Selection.objects[i], false);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        else if (Selection.activeObject != null)
        {
            ImportNewStage_SingleSelected(Selection.activeObject, true);
        }
        else
        {
            Debug.LogWarning("No active object selected. Select a single PNG, JPEG file to create a new stage.");
        }
    }

    static void ImportNewStage_SingleSelected(Object targetObject, bool showConfirm)
    {
        var assetPath = AssetDatabase.GetAssetPath(targetObject);
        if (assetPath.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) == false
            && assetPath.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) == false
            && assetPath.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) == false)
        {
            Debug.LogWarning("Select a single PNG, JPEG file to create a new stage.");
            return;
        }

        var assetPathParent = Path.GetDirectoryName(assetPath);
        var stageName = Path.GetFileNameWithoutExtension(assetPathParent);

        if (showConfirm && !EditorUtility.DisplayDialog($"Import New Stage: {stageName}",
                "This takes about 30-60 seconds to finish. Proceed?", "Proceed", "Cancel"))
        {
            return;
        }

        Program.Main(new[] {"dit", assetPath, "30", "", "", stageName});

        AssetDatabase.Refresh();

        var assetDir = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(assetDir))
        {
            Debug.LogError("Asset directory cannot be determined.");
            return;
        }

        var rawStageDataPath = Path.Combine(assetDir, $"{stageName}.bytes");
        var rawStageData = AssetDatabase.LoadAssetAtPath<TextAsset>(rawStageDataPath);
        ImportStageFromRawStageData(rawStageData);
    }

    [MenuItem("Assets/Black/Import New Stage (Raw Data)")]
    [UsedImplicitly]
    static void ImportNewStage_RawData()
    {
        foreach (var o in Selection.objects)
        {
            if (!(o is TextAsset rawStageData)) continue;

            ImportStageFromRawStageData(rawStageData);
        }
    }

    static void ImportStageFromRawStageData(TextAsset rawStageData)
    {
        var rawStageDataFullPath = AssetDatabase.GetAssetPath(rawStageData);
        var stageDir = Path.GetDirectoryName(rawStageDataFullPath);
        Debug.Log(stageDir);
        if (string.IsNullOrEmpty(stageDir))
        {
            Debug.LogError("Stage directory is empty.");
            return;
        }

        var stageName = Path.GetFileNameWithoutExtension(stageDir);
        if (string.IsNullOrEmpty(stageName))
        {
            Debug.LogError("Stage name cannot be determined.");
            return;
        }

        var metadataName = Path.GetFileNameWithoutExtension(rawStageDataFullPath);
        if (string.IsNullOrEmpty(metadataName))
        {
            Debug.LogError("Metadata name cannot be determined.");
            return;
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
            return;
        }

        if (fsnbPreset == null)
        {
            Debug.LogError($"FSNB Preset not found on path '{fsnbPresetPath}'");
            return;
        }

        if (sdfTex == null)
        {
            Debug.LogError($"SDF Texture not found on path '{sdfTexPath}'");
            return;
        }

        if (fsnbTex == null)
        {
            Debug.LogError($"FSNB Texture not found on path '{fsnbTexPath}'");
            return;
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
        var oldStageMetadata = AssetDatabase.LoadAssetAtPath<StageMetadata>(stageMetadataPath);
        if (oldStageMetadata == null)
        {
            var stageMetadata = StageMetadata.Create(skipBlackMat, sdfMat, rawStageData, stageName);
            AssetDatabase.CreateAsset(stageMetadata, stageMetadataPath);
        }

        Debug.Log($"{stageName} stage created.");

        AssetDatabase.ImportAsset(stageDir, ImportAssetOptions.ImportRecursive);
    }
}