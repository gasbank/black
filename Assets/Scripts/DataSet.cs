using MessagePack;
using System.Collections.Generic;

[System.Serializable]
[MessagePackObject(true)]
public class DataSet
{
    public List<StageSequenceData> StageSequenceData;
    public Dictionary<ScString, StrBaseData> StrKoData; // 한국어
    public Dictionary<ScString, StrBaseData> StrChData; // 중국어 (간체)
    public Dictionary<ScString, StrBaseData> StrTwData; // 중국어 (번체)
    public Dictionary<ScString, StrBaseData> StrJaData; // 일본어
    public Dictionary<ScString, StrBaseData> StrEnData; // 영어
    public List<DailyRewardData> DailyRewardData;

    // 이하는 실행 시 생성되는 데이터

    [IgnoreMember]
    public List<StageMetadata> StageMetadataList;

    [IgnoreMember]
    public Dictionary<string, StageMetadata> StageMetadataDict;
}