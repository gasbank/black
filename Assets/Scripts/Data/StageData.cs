using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class StageData
{
    // 색상 별 섬 수
    public Dictionary<uint, int> islandCountByColor = new Dictionary<uint, int>();

    // Min Point 별 섬 데이터
    public Dictionary<uint, IslandData> islandDataByMinPoint = new Dictionary<uint, IslandData>();

    public uint[] CreateColorUintArray() => islandDataByMinPoint
        .Select(e => e.Value.rgba)
        .Distinct()
        .OrderBy(e => e)
        .ToArray();
}