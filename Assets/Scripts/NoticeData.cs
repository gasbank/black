using System;
using UnityEngine;

[Serializable]
public class NoticeData {
    public string title; // 공지 제목 (일반적으로 '공지사항')
    public string text; // 공지 본문
    public string detailUrl; // 자세히 보기 URL (옵션; 주로 컬러뮤지엄 공식 카페 URL)
    public int eventDrawRiceRateRatio = 1;

    public int GetEventDrawRiceRateRatio(NetworkTime networkTime) {
        if (networkTime.EstimatedNetworkTimeInBetween(EventDrawRiceRateRatioBegin, EventDrawRiceRateRatioEnd)) {
            return Mathf.Max(1, eventDrawRiceRateRatio);
        } else {
            return 1;
        }
    }

    public void SetEventDrawRiceRateRatio(int v) {
        eventDrawRiceRateRatio = v;
    }

    [SerializeField]
    public UDateTime eventDrawRiceRateRatioBegin = NetworkTime.ParseExactUtc("0000-01-01T00:00:00Z");

    public DateTime EventDrawRiceRateRatioBegin {
        get => eventDrawRiceRateRatioBegin?.dateTime ?? DateTime.MinValue;
        set => eventDrawRiceRateRatioBegin = value;
    }

    [SerializeField]
    public UDateTime eventDrawRiceRateRatioEnd = NetworkTime.ParseExactUtc("0000-01-01T00:00:00Z");

    public DateTime EventDrawRiceRateRatioEnd {
        get => eventDrawRiceRateRatioEnd?.dateTime ?? DateTime.MinValue;
        set => eventDrawRiceRateRatioEnd = value;
    }
}