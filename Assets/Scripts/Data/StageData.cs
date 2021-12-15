using System;
using System.Collections.Generic;

[Serializable]
public class StageData
{
    // 색상 별 섬 수
    public Dictionary<uint, int> islandCountByColor = new Dictionary<uint, int>();

    // Min Point 별 섬 데이터
    public Dictionary<uint, IslandData> islandDataByMinPoint = new Dictionary<uint, IslandData>();
}