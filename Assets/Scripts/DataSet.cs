using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;

#if ADDRESSABLES
using UnityEngine.ResourceManagement.ResourceLocations;
#endif

[Serializable]
[MessagePackObject(true)]
public class DataSet
{
    public List<DailyRewardData> DailyRewardData;
    public List<AchievementData> AchievementData_MaxBlackLevel;
    public List<AchievementData> AchievementData_MaxColoringCombo;
    public List<StageSequenceData> StageSequenceData;
    public Dictionary<ScString, StrBaseData> StrChData; // 중국어 (간체)
    public Dictionary<ScString, StrBaseData> StrEnData; // 영어
    public Dictionary<ScString, StrBaseData> StrJaData; // 일본어
    public Dictionary<ScString, StrBaseData> StrKoData; // 한국어
    public Dictionary<ScString, StrBaseData> StrTwData; // 중국어 (번체)

    // 이하의 데이터는 런타임에 정해진다.
    [IgnoreMember]
    public Dictionary<string, IResourceLocation> StageMetadataLocDict;

    [IgnoreMember]
    public List<IResourceLocation> StageMetadataLocList;
}

#if !ADDRESSABLES
public interface IResourceLocation
{
    string PrimaryKey { get; set; }
}

public class DummyResourceLocation : IResourceLocation
{
    public string PrimaryKey { get; set; }
}

public static class Addressables
{
    public static AddressablesListTask<IResourceLocation[]> LoadResourceLocationsAsync(string stage, Type type)
    {
        var r = new AddressablesListTask<IResourceLocation[]>();
        return r;
    }

    public static AddressablesTask<T> LoadAssetAsync<T>(IResourceLocation loc) where T : UnityEngine.Object
    {
        var r = new AddressablesTask<T>();
        return r;
    }
}

public class AddressablesListTask<T> where T : IEnumerable
{
    public Task<DummyResourceLocation[]> Task;

    public AddressablesListTask()
    {
        Task = System.Threading.Tasks.Task.Run(() =>
        {
            var x = new List<DummyResourceLocation>();
            return x.ToArray();
        });
    }
}

public class AddressablesTask<T> where T : UnityEngine.Object
{
    public System.Threading.Tasks.Task<T> Task;
}
#endif