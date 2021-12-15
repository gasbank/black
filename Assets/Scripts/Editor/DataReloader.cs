using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using MessagePack;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using ConditionalDebug;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

public static class DataReloader
{
    static readonly string[] fullXlsxList =
    {
        "Data/StageSequence.xlsx",
        "Data/DailyReward.xlsx",
        "Data/StrKo.xlsx",
        "Data/StrEn.xlsx",
    };

    [MenuItem("Black/Reload Data")]
    [UsedImplicitly]
    static void ReloadData()
    {
        var xlsxList = GetUpdatedXlsxList();
        ReloadData(xlsxList, false);
        CheckLocalizedKeyConsistency();
    }

    static void CheckLocalizedKeyConsistency()
    {
        var dataSet = Data.LoadSharedDataSet();

        // 번역 텍스트 키와 원본(한국어) 텍스트 키의 차이가 발견되면 분명한 오류다.
        var strKoDataKeys = new HashSet<string>(dataSet.StrKoData.Keys.Select(e => e.ToString()));
        var strEnDataKeys = new HashSet<string>(dataSet.StrEnData.Keys.Select(e => e.ToString()));

        // 한국어 텍스트는 있는데, 다른 언어 텍스트가 없는 경우 출력
        foreach (var koKey in strKoDataKeys)
        {
            CheckStrKeyExists(strEnDataKeys, "En", koKey);
        }

        // 한국어 텍스트는 없는데, 다른 언어 텍스트에 있는 경우 출력
        CheckStrKeyUnused(strEnDataKeys, "En", strKoDataKeys);
    }

    static void CheckStrKeyUnused(HashSet<string> keys, string langName, HashSet<string> strKoDataKeys)
    {
        foreach (string key in keys.Except(strKoDataKeys))
        {
            Debug.LogWarning($"String Key {key}' exists in {langName} but not in Ko! (dangling key)");
        }
    }

    private static List<string> GetUpdatedXlsxList()
    {
        var hashMsgPackBytes = Resources.Load("Data/Hash-MsgPack") as TextAsset;
        List<string> xlsxList = new List<string>();
        if (hashMsgPackBytes != null)
        {
            Data.RegisterAllResolversOnce();

            //var hash = LZ4MessagePackSerializer.Deserialize<DataSetHash>(hashMsgPackBytes.bytes);
            var hash = MessagePackSerializer.Deserialize<DataSetHash>(hashMsgPackBytes.bytes, Data.DefaultOptions);

            var currentHash = fullXlsxList.Select(e => new[] {e, CalculateMd5(e)}).ToDictionary(e => e[0], e => e[1]);

            foreach (var kv in currentHash)
            {
                if (hash.Hash.TryGetValue(kv.Key, out var hashValue) == false || hashValue != kv.Value)
                {
                    xlsxList.Add(kv.Key);
                }
            }
        }
        else
        {
            xlsxList = new List<string>(fullXlsxList);
        }

        return xlsxList;
    }

    static void ReloadData(ICollection<string> xlsxList, bool force)
    {
        try
        {
            ReloadDataSafe(xlsxList, force);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static void ReloadDataSafe(ICollection<string> xlsxList, bool force)
    {
        if (xlsxList.Count == 0)
        {
            Debug.Log("Already up-to-date.");
            return;
        }

        var dataSet = new DataSet();

        if (force == false)
        {
            EditorUtility.DisplayProgressBar("Reload Data", "Loading the previous DataSet", 0);
            dataSet = Data.LoadSharedDataSet();
        }

        var csvForDiffPath = Path.Combine("Data", "CsvForDiff");
        Directory.CreateDirectory(csvForDiffPath);

        var xlsxIndex = 0;

        foreach (var xlsx in xlsxList)
        {
            ConDebug.Log($"Loading {xlsx}...");
            EditorUtility.DisplayProgressBar("Reload Data", xlsx, (xlsxIndex + 1.0f) / xlsxList.Count);
            var tableList = new ExcelToObject.TableList(xlsx);
            tableList.MapInto(dataSet);
            xlsxIndex++;

            Xlsx2Csv20.Xlsx2Csv.Convert(xlsx,
                Path.Combine(csvForDiffPath, Path.GetFileNameWithoutExtension(xlsx) + ".csv"));
        }

        // 번역되는 텍스트는 diff를 쉽게 볼 수 있도록 텍스트파일로도 쓴다.
        WriteStringDataToTextFile(Path.Combine("Data", "StrKoData.txt"), dataSet.StrKoData, true, 0.2f);
        WriteStringDataToTextFile(Path.Combine("Data", "StrEnData.txt"), dataSet.StrEnData, true, 1.0f);
        
        ConDebug.Log($"{dataSet.StageSequenceData.Count} entries on StageSequenceData");

        EditorUtility.DisplayProgressBar("Reload Data", "Serializing and writing...", 0.0f);

        SerializeAndWriteDataSet(dataSet);

        EditorUtility.ClearProgressBar();

        if (force == false)
        {
            Debug.Log("Updated Xlsx List");
            foreach (var xlsx in xlsxList)
            {
                Debug.Log($"- {xlsx}");
            }
        }
    }

    static void CheckStrKeyExists(HashSet<string> keys, string langName, string koKey)
    {
        if (keys.Contains(koKey) == false)
        {
            Debug.LogError($"String Key '{koKey}' exists in Ko but not in {langName}!");
        }
    }

    // https://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file
    static string CalculateMd5(string filename)
    {
        using var md5 = MD5.Create();
        using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var hash = md5.ComputeHash(stream);
        return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static void SerializeAndWriteDataSet(DataSet dataSet)
    {
        Data.RegisterAllResolversOnce();

        var hashMsgPackDataPath = "Assets/Resources/Data/Hash-MsgPack.bytes";
        var dataSetHash = new DataSetHash
            {Hash = fullXlsxList.Select(e => new[] {e, CalculateMd5(e)}).ToDictionary(e => e[0], e => e[1])};
        var hashSerialized = Data.SerializeDataSetWithComparison(dataSetHash);
        File.WriteAllBytes(hashMsgPackDataPath, hashSerialized);

        var blackMsgPackDataPath = "Assets/Resources/Data/Black-MsgPack.bytes";
        BlackStringTable.StringNumberDict.Clear();
        var serialized = Data.SerializeDataSetWithComparison(dataSet);
        File.WriteAllBytes(blackMsgPackDataPath, serialized);
        Debug.Log($"String Dict Count: {BlackStringTable.StringNumberDict.Count:n0}");

        EditorUtility.DisplayProgressBar("Reload Data", "Serializing and writing...", 0.5f);

        var stringList = BlackStringTable.StringNumberDict.OrderBy(e => e.Value).Select(e => e.Key).ToArray();
        //var strSerializedNotCompressed = MessagePackSerializer.Serialize(stringList, mutableCompositeResolver);
        var strSerializedNotCompressed = MessagePackSerializer.Serialize(stringList, Data.BlackStrNoCompOptions);
        //var strSerialized = LZ4MessagePackSerializer.Serialize(stringList, mutableCompositeResolver);
        var strSerialized = MessagePackSerializer.Serialize(stringList, Data.BlackStrOptions);
        var blackStrMsgPackDataPath = "Assets/Resources/Data/BlackStr-MsgPack.bytes";
        File.WriteAllBytes(blackStrMsgPackDataPath, strSerialized);

        EditorUtility.DisplayProgressBar("Reload Data", "Serializing and writing...", 1.0f);

        Debug.Log("=== String Table Serialization Result ===");
        Debug.Log($"  Before compression: {strSerializedNotCompressed.Length:n0} bytes");
        Debug.Log($"  After compression: {strSerialized.Length:n0} bytes");
        Debug.Log($"  Compression ratio: {(float) strSerialized.Length / strSerializedNotCompressed.Length:f2}");

        Data.DeserializeDataSet(serialized, strSerialized);
        Debug.Log("Deserialized successfully.");

        AssetDatabase.Refresh();
    }

    [MenuItem("Black/Generate textref.txt")]
    [UsedImplicitly]
    static void GatherAllStaticLocalizedTextRef()
    {
        List<string> textRefList = new List<string>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (var staticLocalizedText in root.GetComponentsInChildren<StaticLocalizedText>(true))
            {
                textRefList.Add(staticLocalizedText.StrRef);
            }
        }

        textRefList = textRefList.Distinct().ToList();
        textRefList.Sort();
        File.WriteAllLines("textref.txt", textRefList.ToArray());
        ConDebug.LogFormat("textref.txt written: {0} items", textRefList.Count);
    }

    [MenuItem("Black/Print All Text Sizes")]
    [UsedImplicitly]
    static void PrintAllTextSizes()
    {
        List<string> lines = new List<string>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (var text in root.GetComponentsInChildren<Text>(true))
            {
                lines.Add(
                    $"{text.fontSize}\t{text.gameObject.name}\t{text.text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t")}");
            }
        }

        File.WriteAllLines("all-text-sizes.txt", lines.OrderBy(e => e));
    }

    static void WriteStringDataToTextFile(string path, Dictionary<ScString, StrBaseData> strData, bool appendAscii,
        float progress)
    {
        EditorUtility.DisplayProgressBar("Reload Data", path, progress);

        ConDebug.Log($"Writing {path}...");
        StringBuilder sb = new StringBuilder();
        var orderedKeys = strData.Keys.OrderBy(NormalizeLineEndings, System.StringComparer.Ordinal);
        foreach (var k in orderedKeys)
        {
            foreach (var s in strData[k].str)
            {
                if (s != null && s.ToString().Length > 0)
                {
                    string normalized = NormalizeLineEndings(s.ToString());
                    sb.AppendLine(normalized.Replace("\r\n", @"\n"));
                }
            }
        }

        if (appendAscii)
        {
            // 0x20: Blank character
            // 0x7e: ~
            // 0x7f: [Delete] character (special character)
            sb.AppendLine(string.Concat(Enumerable.Range('\x20', '\x7e' - '\x20' + 1).Select(e => (char) e)));
        }

        File.WriteAllText(path, sb.ToString());
    }

    private static string NormalizeLineEndings(ScString e)
    {
        return Regex.Replace(e.ToString(), @"\r\n|\n\r|\n|\r", "\r\n");
    }

    [MenuItem("Black/Reload Data (Force)")]
    [UsedImplicitly]
    static void ReloadDataForce()
    {
        ReloadData(fullXlsxList, true);
        CheckLocalizedKeyConsistency();
    }

    [MenuItem("Black/List All Scaled Objects")]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    [UsedImplicitly]
    static void ListAllScaledObjects()
    {
        var transformList = SceneManager.GetActiveScene().GetRootGameObjects()
            .Select(e => e.GetComponentsInChildren<Transform>()).SelectMany(e => e);
        var xFlipped = new Vector3(-1, 1, 1);
        var yZero = new Vector3(1, 0, 1);
        foreach (var transform in transformList)
        {
            var localScale = transform.localScale;

            if (transform.GetComponent<Canvas>() == null
                && localScale != Vector3.zero
                && localScale != Vector3.one
                && localScale != 2.0f * Vector3.one
                && localScale != 0.3f * Vector3.one
                && localScale != 0.7f * Vector3.one
                && localScale != 0.8f * Vector3.one
                && localScale != 1.7f * Vector3.one
                && localScale != 1.8f * Vector3.one
                && localScale != xFlipped
                && localScale != yZero)
            {
                Debug.Log($"{transform.name}: scaled to {localScale}", transform.gameObject);
            }
        }
    }

    [MenuItem("Black/Delete All Save Files")]
    [UsedImplicitly]
    static void DeleteAllSaveFiles()
    {
        SaveLoadManager.DeleteAllSaveFiles();
    }
}