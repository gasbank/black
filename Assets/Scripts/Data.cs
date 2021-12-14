using System.Collections.Generic;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;
using UnityEngine.Profiling;

[DisallowMultipleComponent]
public class Data : MonoBehaviour
{
    public static Data instance;

    public static DataSet dataSet;
    
    // 업적(컨텐츠 상 퀘스트) 종류별로 정렬된 리스트 (데이터시트상에서 정렬되어 있어야 함)
    public static readonly Dictionary<ScString, List<AchievementData>> achievementOrderedGroup =
        new Dictionary<ScString, List<AchievementData>>();

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

    void Start()
    {
        AssignSharedDataSetConditional();
    }

    static void AssignSharedDataSetConditional()
    {
        if (dataSet != null) return;
        
        dataSet = LoadSharedDataSet();
        Profiler.BeginSample("Prebuild Dependent DataSet");
        PrebuildDependentDataSet(dataSet);
        Profiler.EndSample();
    }

    static void PrebuildDependentDataSet(DataSet dataSet1)
    {
    }

    public static DataSet LoadSharedDataSet()
    {
        RegisterAllResolversOnce();
        Profiler.BeginSample("Load Black-MsgPack");
        var blackMsgPackBytes = Resources.Load("Data/Black-MsgPack") as TextAsset;
        Profiler.EndSample();
        Profiler.BeginSample("Load BlackStr-MsgPack");
        var blackStrMsgPackBytes = Resources.Load("Data/BlackStr-MsgPack") as TextAsset;
        Profiler.EndSample();
        Profiler.BeginSample("Deserialize DataSet");
        var dataSet = DeserializeDataSet(blackMsgPackBytes.bytes, blackStrMsgPackBytes.bytes);
        Profiler.EndSample();
        return dataSet;
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

    public BlackLanguageCode CurrentLanguageCode = BlackLanguageCode.Ko;

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
            //MessagePack.Resolvers.GeneratedResolver.Instance,
            BuiltinResolver.Instance));

    public static readonly MessagePackSerializerOptions BlackStrOptions =
        BlackStrNoCompOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

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

        Debug.Log($"=== Serialization Result ===");
        Debug.Log($"  Before compression: {notCompressed.Length:n0} bytes");
        Debug.Log($"  After compression: {compressed.Length:n0} bytes");
        Debug.Log($"  Compression ratio: {(float) compressed.Length / notCompressed.Length:f2}");
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