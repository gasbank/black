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

internal interface IAsyncOperation
{
    System.Threading.Tasks.Task<object> Task { get; }
    int Version { get; }
}

public abstract class AsyncOperationBase<TObject> : IAsyncOperation
{
    System.Threading.Tasks.Task<object> IAsyncOperation.Task { get; }
    public virtual int Version { get; }
    public virtual System.Threading.Tasks.Task<TObject> Task { get; }
}

class ResourceLocationOperation : AsyncOperationBase<IList<IResourceLocation>>
{
    public override int Version => 1985;

    public override Task<IList<IResourceLocation>> Task
    {
        get
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                var list = new List<IResourceLocation>();
                return (IList<IResourceLocation>) list;
            });
        }
    }
}

public struct AsyncOperationHandle<TObject> : IEnumerator, IEquatable<AsyncOperationHandle<TObject>>
{
    readonly int m_Version;
    readonly AsyncOperationBase<TObject> m_InternalOp;
    
    internal AsyncOperationHandle(IAsyncOperation op, string locationName)
    {
        m_Version = 1985;
        m_InternalOp = (AsyncOperationBase<TObject>) op;
    }

    bool IEnumerator.MoveNext()
    {
        throw new NotImplementedException();
    }

    void IEnumerator.Reset()
    {
        throw new NotImplementedException();
    }

    object IEnumerator.Current
    {
        get { return Result; }
    }
    
    public object Result
    {
        get { return null; }
    }
    
    AsyncOperationBase<TObject> InternalOp
    {
        get
        {
            if (m_InternalOp == null || m_InternalOp.Version != m_Version)
                throw new Exception("Attempting to use an invalid operation handle");
            return m_InternalOp;
        }
    }
    
    public bool Equals(AsyncOperationHandle<TObject> other)
    {
        return m_Version == other.m_Version && m_InternalOp == other.m_InternalOp;
    }
    
    public System.Threading.Tasks.Task<TObject> Task => InternalOp.Task;
}

public static class Addressables
{
    public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(object key, Type type = null)
    {
        var op = new ResourceLocationOperation();
        var r = new AsyncOperationHandle<IList<IResourceLocation>>(op, (string) key);
        return r;
    }

    public static AddressablesTask<T> LoadAssetAsync<T>(IResourceLocation loc) where T : UnityEngine.Object
    {
        var r = new AddressablesTask<T>();
        return r;
    }
    
}

public class AddressablesTask<T> where T : UnityEngine.Object
{
    public System.Threading.Tasks.Task<T> Task;
}
#endif