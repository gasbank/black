using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using black_dev_tools;

internal static class StageEditorUtil
{
    static readonly int SdfTexture = Shader.PropertyToID("SdfTexture");
    static readonly int ColorTexture = Shader.PropertyToID("ColorTexture");

    [MenuItem("Assets/Black/Import New Stage (PNG Image)")]
    [UsedImplicitly]
    static void ImportNewStage_PngImage()
    {
        if (Selection.activeObject == null)
        {
            Debug.LogWarning("No active object selected. Select a single PNG file to create a new stage.");
            return;
        }

        var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (assetPath.EndsWith(".png") == false)
        {
            Debug.LogWarning("Select a single PNG file to create a new stage.");
            return;
        }

        if (!EditorUtility.DisplayDialog("Import New Stage",
            "This takes 30-60 seconds to finish. Proceed?", "Proceed", "Cancel"))
        {
            return;
        }
        
        Program.Main(new[] {"dit", assetPath, "30"});

        AssetDatabase.Refresh();

        var assetDir = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(assetDir))
        {
            Debug.LogError("Asset directory cannot be determined.");
            return;
        }

        var stageName = Path.GetFileNameWithoutExtension(assetPath);
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
        var stageMetadata = StageMetadata.Create(skipBlackMat, sdfMat, rawStageData, stageName);
        AssetDatabase.CreateAsset(stageMetadata, stageMetadataPath);

        Debug.Log($"{stageName} stage created.");
    }
}