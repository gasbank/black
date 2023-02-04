using System.IO;
using UnityEditor;
using UnityEngine;

public static class KoreanUtil
{
    [MenuItem("Assets/Black/자소 분리 문제 수정하기")]
    static void FixFileKoreanFileNamesMultipleSelection()
    {
        foreach (var o in Selection.objects)
        {
            NormalizeFileName(o);
            EditorUtility.SetDirty(o);
        }

        AssetDatabase.Refresh();
    }

    static void NormalizeFileName(UnityEngine.Object obj)
    {
        var assetPath = AssetDatabase.GetAssetPath(obj);
        var assetPathNormalized = assetPath.Normalize();

        if (string.CompareOrdinal(assetPath, assetPathNormalized) != 0)
        {
            Debug.Log($"Normalizing graph file '{assetPath}' to '{assetPathNormalized}'...");

            // AssetDatabase.RenameAsset()으로는 해결되지 않는다.    
            File.Move(assetPath, assetPathNormalized);
        }
    }
}
