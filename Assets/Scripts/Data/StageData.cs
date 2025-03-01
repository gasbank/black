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

    List<MinPointIslandData> cachedIslandDataList;

    uint[] cachedColorUintArray;

    public struct MinPointIslandData
    {
        public MinPointIslandData(uint minPoint, IslandData islandData)
        {
            MinPoint = minPoint;
            IslandData = islandData;
        }

        public readonly uint MinPoint;
        public readonly IslandData IslandData;
    }

    public List<MinPointIslandData> CachedIslandDataList =>
        cachedIslandDataList ??= islandDataByMinPoint
            .Select(e => new MinPointIslandData(e.Key, e.Value))
            .ToList();

    public uint[] CachedPaletteArray =>
        cachedColorUintArray ??= CreateColorUintArray();

    public uint[] CreateColorUintArray() => islandDataByMinPoint
        .Select(e => e.Value.rgba)
        .Distinct()
        .OrderBy(e => e)
        .ToArray();
}