using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IslandLabelSpawner : MonoBehaviour {
    [SerializeField] GameObject islandLabelNumberPrefab = null;
    [SerializeField] GridWorld gridWorld = null;
    [SerializeField] RectTransform rt = null;
    [SerializeField] PaletteButtonGroup paletteButtonGroup = null;
    [SerializeField] Transform islandLabelNumberGroup = null;

    Dictionary<uint, IslandLabel> labelByMinPoint = new Dictionary<uint, IslandLabel>();

    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }

    RectInt GetRectRange(ulong maxRectUlong) {
        var xMin = (int)(maxRectUlong & 0xffff);
        var yMax = gridWorld.texSize - (int)((maxRectUlong >> 16) & 0xffff);
        var xMax = (int)((maxRectUlong >> 32) & 0xffff);
        var yMin = gridWorld.texSize - (int)((maxRectUlong >> 48) & 0xffff);
        var r = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        //Debug.Log($"{r.xMin},{r.yMin} -- {r.xMax},{r.yMax} (area={r.size.x * r.size.y})");
        return r;
    }

    public void CreateAllLabels(StageData stageData) {
        var maxRectDict = stageData.islandDataByMinPoint.ToDictionary(e => e.Key, e => GetRectRange(e.Value.maxRect));
        //var area = maxRectDict.Select(e => e.Value.size.x * e.Value.size.y).OrderByDescending(e => e).Take(20).LastOrDefault();
        var rectIndex = 0;
        var subgroupCapacity = 50;
        GameObject islandLabelNumberSubgroup = null;
        foreach (var kv in maxRectDict) {
            //Debug.Log($"Big sub rect island: ({kv.Value.xMin},{kv.Value.yMin})-({kv.Value.xMax},{kv.Value.yMax}) area={kv.Value.size.x * kv.Value.size.y}");
            if (rectIndex % subgroupCapacity == 0) {
                islandLabelNumberSubgroup = new GameObject($"Island Label Subgroup ({rectIndex:d4}-{rectIndex+subgroupCapacity-1:d4})");
                islandLabelNumberSubgroup.transform.parent = islandLabelNumberGroup;
                var subGroupRt = islandLabelNumberSubgroup.AddComponent<RectTransform>();
                subGroupRt.anchoredPosition3D = Vector3.zero;
                subGroupRt.localScale = Vector3.one;
            }
            var label = Instantiate(islandLabelNumberPrefab, islandLabelNumberSubgroup.transform).GetComponent<IslandLabel>();
            var labelRt = label.Rt;
            var texSizeFloat = (float)gridWorld.texSize;
            var anchoredPosition = kv.Value.center / texSizeFloat * rt.sizeDelta - rt.sizeDelta / 2;
            labelRt.anchoredPosition = anchoredPosition;
            var sizeDelta = (Vector2)kv.Value.size / texSizeFloat * rt.sizeDelta;
            labelRt.sizeDelta = sizeDelta;
            var paletteIndex = paletteButtonGroup.GetPaletteIndexByColor(stageData.islandDataByMinPoint[kv.Key].rgba);
            label.Text = (paletteIndex + 1).ToString();
            labelByMinPoint[kv.Key] = label;
            label.name = $"Island Label {rectIndex:d4} #{paletteIndex + 1:d2}";
            rectIndex++;
        }
    }

    public void DestroyLabelByMinPoint(uint minPointUint) {
        if (labelByMinPoint.TryGetValue(minPointUint, out var label)) {
            Destroy(label.gameObject);
            labelByMinPoint.Remove(minPointUint);
        } else {
            Debug.LogError($"DestroyLabelByMinPoint: could not find minPointUint {minPointUint}!");
        }
    }

    public void SetLabelBackgroundImageActive(bool b) {
        foreach (var kv in labelByMinPoint) {
            kv.Value.BackgroundImageActive = b;
        }
    }
}
