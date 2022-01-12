using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConditionalDebug;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.ResourceLocations;

[DisallowMultipleComponent]
public class Data : MonoBehaviour
{
    static bool Verbose => false;
    
    public static Data instance;

    public static DataSet dataSet;

    // 업적(컨텐츠 상 퀘스트) 종류별로 정렬된 리스트 (데이터시트상에서 정렬되어 있어야 함)
    public static readonly Dictionary<ScString, List<AchievementData>> achievementOrderedGroup =
        new Dictionary<ScString, List<AchievementData>>();

    public static readonly MessagePackSerializerOptions DefaultNoCompOptions =
        MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
            BlackStringResolver.Instance,
            GeneratedResolver.Instance,
            BuiltinResolver.Instance));

    public static readonly MessagePackSerializerOptions DefaultOptions =
        DefaultNoCompOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

    public static readonly MessagePackSerializerOptions BlackStrNoCompOptions =
        MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
            BlackStringTableResolver.Instance,
            GeneratedResolver.Instance,
            BuiltinResolver.Instance));

    public static readonly MessagePackSerializerOptions BlackStrOptions =
        BlackStrNoCompOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

    public BlackLanguageCode CurrentLanguageCode = BlackLanguageCode.Ko;

    public static DataSet DeserializeDataSet(byte[] buffer, byte[] strBuffer)
    {
        Profiler.BeginSample("Deserialize DataSet (StringMap)");
        BlackStringTable.ScStringMap = MessagePackSerializer.Deserialize<ScString[]>(strBuffer, BlackStrOptions);
        Profiler.EndSample();
        Profiler.BeginSample("Deserialize DataSet");
        var ret = MessagePackSerializer.Deserialize<DataSet>(buffer, DefaultOptions);
        Profiler.EndSample();
        return ret;
    }

    async void Start()
    {
        await AssignSharedDataSetConditionalAsync();
    }

    static async Task AssignSharedDataSetConditionalAsync()
    {
        // 에디터 환경에서는 개발 과정 중이므로 반복해서 실행한다.
        if (Application.isEditor == false && dataSet != null) return;

        dataSet = LoadSharedDataSet();
        Profiler.BeginSample("Prebuild Dependent DataSet");
        await PrebuildDependentDataSetAsync(dataSet);
        Profiler.EndSample();
    }

    static async Task PrebuildDependentDataSetAsync(DataSet newDataSet)
    {
        var stageAssetLocList = await Addressables.LoadResourceLocationsAsync("Stage", typeof(StageMetadata)).Task;

        newDataSet.StageMetadataLocDict = stageAssetLocList.ToDictionary(e => e.PrimaryKey, e => e);
        newDataSet.StageMetadataLocList = new List<IResourceLocation>();

        foreach (var seq in newDataSet.StageSequenceData)
        {
            if (newDataSet.StageMetadataLocDict.TryGetValue(seq.stageName, out var stageMetadata))
            {
                if (Verbose)
                {
                    ConDebug.Log($"Stage: {seq.stageName} - {stageMetadata}");
                }

                newDataSet.StageMetadataLocList.Add(stageMetadata);
            }
            else
            {
                Debug.LogError($"Stage metadata with name {seq.stageName} not found.");
                newDataSet.StageMetadataLocList.Add(null);
            }
        }
    }

    public static DataSet LoadSharedDataSet()
    {
        RegisterAllResolversOnce();

        Profiler.BeginSample("Load Black-MsgPack");
        var blackMsgPackBytes = Resources.Load("Data/Black-MsgPack") as TextAsset;
        Profiler.EndSample();
        if (blackMsgPackBytes == null)
        {
            Debug.LogError("Black-MsgPack not found");
            return null;
        }

        Profiler.BeginSample("Load BlackStr-MsgPack");
        var blackStrMsgPackBytes = Resources.Load("Data/BlackStr-MsgPack") as TextAsset;
        Profiler.EndSample();
        if (blackStrMsgPackBytes == null)
        {
            Debug.LogError("BlackStr-MsgPack not found");
            return null;
        }

        Profiler.BeginSample("Deserialize DataSet");
        var newDataSet = DeserializeDataSet(blackMsgPackBytes.bytes, blackStrMsgPackBytes.bytes);
        Profiler.EndSample();

        return newDataSet;
    }

    public static void RegisterAllResolversOnce()
    {
        // if (MessagePackSerializer.IsInitialized == false) {
        //     // 두 번 호출하면 오류난다.
        //     MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
        //         BlackStringResolver.Instance,
        //         MessagePack.Resolvers.GeneratedResolver.Instance,
        //         MessagePack.Resolvers.BuiltinResolver.Instance
        //     );
        // }
    }

    public static DataSet DeserializeDataSet(byte[] buffer)
    {
        var ret = MessagePackSerializer.Deserialize<DataSet>(buffer, DefaultOptions);
        return ret;
    }

    public static byte[] SerializeDataSetWithComparison<T>(T value)
    {
        BlackStringTable.StringNumberDict.Clear();
        var notCompressed = MessagePackSerializer.Serialize(value, DefaultNoCompOptions);

        BlackStringTable.StringNumberDict.Clear();
        var compressed = MessagePackSerializer.Serialize(value, DefaultOptions);

        ConDebug.Log("=== Serialization Result ===");
        ConDebug.Log($"  Before compression: {notCompressed.Length:n0} bytes");
        ConDebug.Log($"  After compression: {compressed.Length:n0} bytes");
        ConDebug.Log($"  Compression ratio: {(float) compressed.Length / notCompressed.Length:f2}");
        return compressed;
    }

    public static int AchievementData_ConditionNewArg_LowerBound(List<AchievementData> list, long val)
    {
        var first = 0;
        var last = list.Count;
        var count = last - first;
        var it = first;
        var step = 0;
        while (count > 0)
        {
            it = first;
            step = count / 2;
            it += step;
            if (list[it].conditionNewArg < val)
            {
                first = ++it;
                count -= step + 1;
            }
            else
            {
                count = step;
            }
        }

        return first;
    }

    public static int AchievementData_ConditionNewArg_UpperBound(List<AchievementData> list, long val)
    {
        var first = 0;
        var last = list.Count;
        var count = last - first;
        var it = first;
        var step = 0;
        while (count > 0)
        {
            it = first;
            step = count / 2;
            it += step;
            if (!(val < list[it].conditionNewArg))
            {
                first = ++it;
                count -= step + 1;
            }
            else
            {
                count = step;
            }
        }

        return first;
    }
}