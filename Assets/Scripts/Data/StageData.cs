using System;
using System.Collections.Generic;

[Serializable]
public class StageData {
    public Dictionary<uint, IslandData> islandDataByMinPoint = new Dictionary<uint, IslandData>();
}
