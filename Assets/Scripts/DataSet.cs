using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine.ResourceManagement.ResourceLocations;

[Serializable]
[MessagePackObject(true)]
public class DataSet
{
    public List<DailyRewardData> DailyRewardData;

    [IgnoreMember]
    public Dictionary<string, IResourceLocation> StageMetadataDict;

    // 이하는 실행 시 생성되는 데이터

    [IgnoreMember]
    public List<IResourceLocation> StageMetadataList;

    public List<StageSequenceData> StageSequenceData;
    public Dictionary<ScString, StrBaseData> StrChData; // 중국어 (간체)
    public Dictionary<ScString, StrBaseData> StrEnData; // 영어
    public Dictionary<ScString, StrBaseData> StrJaData; // 일본어
    public Dictionary<ScString, StrBaseData> StrKoData; // 한국어
    public Dictionary<ScString, StrBaseData> StrTwData; // 중국어 (번체)
}