using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine.ResourceManagement.ResourceLocations;

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