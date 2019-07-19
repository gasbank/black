using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IslandLabelSpawner : MonoBehaviour {
    [SerializeField] GameObject islandLabelNumberPrefab = null;
    [SerializeField] GridWorld gridWorld = null;
    [SerializeField] RectTransform rt = null;

    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }

    RectInt GetRectRange(ulong maxRectUlong) {
        var xMin = (int)(maxRectUlong & 0xffff);
        var yMax = gridWorld.texSize - (int)((maxRectUlong >> 16) & 0xffff) - 1;
        var xMax = (int)((maxRectUlong >> 32) & 0xffff);
        var yMin = gridWorld.texSize - (int)((maxRectUlong >> 48) & 0xffff) - 1;
        var r = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        //Debug.Log($"{r.xMin},{r.yMin} -- {r.xMax},{r.yMax} (area={r.size.x * r.size.y})");
        return r;
    }

    public void CreateAllLabels(StageData stageData) {
        var maxRectDict = stageData.islandDataByMinPoint.ToDictionary(e => e.Key, e => GetRectRange(e.Value.maxRect));
        var area = maxRectDict.Select(e => e.Value.size.x * e.Value.size.y).OrderByDescending(e => e).Take(20).LastOrDefault();
        foreach (var kv in maxRectDict) {
            if (kv.Value.size.x * kv.Value.size.y >= area) {
                Debug.Log($"Big sub rect island: ({kv.Value.xMin},{kv.Value.yMin})-({kv.Value.xMax},{kv.Value.yMax}) area={kv.Value.size.x * kv.Value.size.y}");
                var label = Instantiate(islandLabelNumberPrefab, transform);
                var labelRt = label.GetComponent<RectTransform>();
                var texSizeFloat = (float)gridWorld.texSize;
                labelRt.anchoredPosition = new Vector2(kv.Value.center.x / texSizeFloat * rt.sizeDelta.x - rt.sizeDelta.x / 2, kv.Value.center.y / texSizeFloat * rt.sizeDelta.y - rt.sizeDelta.y / 2);
            }
        }
    }
}
