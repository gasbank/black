using System;
using MessagePack;
using UnityEngine;

[Serializable]
[MessagePackObject]
public class NoticeData
{
    [Key(2)]
    public string detailUrl; // 자세히 보기 URL (옵션; 주로 컬러뮤지엄 공식 카페 URL)

    [Key(3)]
    public int eventDrawGoldRateRatio = 1;

    [SerializeField]
    [Key(4)]
    public UDateTime eventDrawGoldRateRatioBegin = NetworkTime.ParseExactUtc("0000-01-01T00:00:00Z");

    [SerializeField]
    [Key(5)]
    public UDateTime eventDrawGoldRateRatioEnd = NetworkTime.ParseExactUtc("0000-01-01T00:00:00Z");

    [Key(1)]
    public string text; // 공지 본문

    [Key(0)]
    public string title; // 공지 제목 (일반적으로 '공지사항')

    [IgnoreMember]
    public DateTime EventDrawGoldRateRatioBegin
    {
        get => eventDrawGoldRateRatioBegin?.dateTime ?? DateTime.MinValue;
        set => eventDrawGoldRateRatioBegin = value;
    }

    [IgnoreMember]
    public DateTime EventDrawGoldRateRatioEnd
    {
        get => eventDrawGoldRateRatioEnd?.dateTime ?? DateTime.MinValue;
        set => eventDrawGoldRateRatioEnd = value;
    }

    public int GetEventDrawGoldRateRatio(NetworkTime networkTime)
    {
        if (networkTime.EstimatedNetworkTimeInBetween(EventDrawGoldRateRatioBegin, EventDrawGoldRateRatioEnd))
            return Mathf.Max(1, eventDrawGoldRateRatio);
        return 1;
    }

    public void SetEventDrawGoldRateRatio(int v)
    {
        eventDrawGoldRateRatio = v;
    }
}