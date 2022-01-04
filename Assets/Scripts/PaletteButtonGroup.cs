using System;
using System.Collections.Generic;
using System.Linq;
using ConditionalDebug;
using UnityEngine;

public class PaletteButtonGroup : MonoBehaviour
{
    static bool Verbose => false;

    [SerializeField]
    List<PaletteButton> paletteButtonList;

    [SerializeField]
    PaletteButton paletteButtonPrefab;

    [SerializeField]
    GameObject poofPrefab;

    readonly Dictionary<uint, int> paletteIndexbyColor = new Dictionary<uint, int>();

    StageData stageData;

    void Awake()
    {
        DestroyAllPaletteButtons();
    }

    public Color CurrentPaletteColor
    {
        get
        {
            foreach (Transform t in transform)
            {
                var pb = t.GetComponent<PaletteButton>();
                if (pb.Check) return pb.PaletteColor;
            }

            return Color.white;
        }
    }

    public uint CurrentPaletteColorUint
    {
        get
        {
            foreach (Transform t in transform)
            {
                var pb = t.GetComponent<PaletteButton>();
                if (pb.Check)
                {
                    if (Verbose) ConDebug.Log($"CurrentPaletteColorUint: {pb.ColorUint} (0x{pb.ColorUint:X8})");
                    return pb.ColorUint;
                }
            }

            return 0xffffffff;
        }
    }

    public void CreatePalette(StageData stageData)
    {
        DestroyAllPaletteButtons();

        var colorUintArray = stageData.islandDataByMinPoint.Select(e => e.Value.rgba).Distinct().OrderBy(e => e)
            .ToArray();
        var paletteIndex = 0;
        paletteButtonList.Clear();
        foreach (var colorUint in colorUintArray)
        {
            if ((colorUint & 0x00ffffff) == 0x00ffffff)
                Debug.LogError("CRITICAL ERROR: Palette color cannot be WHITE!!!");
            var paletteButton = Instantiate(paletteButtonPrefab, transform).GetComponent<PaletteButton>();
            paletteButton.SetColor(colorUint);
            paletteIndexbyColor[colorUint] = paletteIndex;
            paletteButton.ColorIndex = paletteIndex + 1;
            paletteIndex++;
            paletteButtonList.Add(paletteButton);
        }

        // 첫 번째 팔레트 기본 선택 상태
        if (paletteButtonList.Count > 0)
        {
            paletteButtonList[0].Check = true;
        }

        this.stageData = stageData;
    }

    void DestroyAllPaletteButtons()
    {
        foreach (var t in transform.Cast<Transform>().ToArray()) Destroy(t.gameObject);
    }

    public int GetPaletteIndexByColor(uint color)
    {
        return paletteIndexbyColor[color];
    }

    public void UpdateColoredCount(uint color, int count, bool batch)
    {
        var paletteIndex = GetPaletteIndexByColor(color);
        var paletteButton = paletteButtonList[paletteIndex];

        var oldRatio = paletteButton.ColoredRatio;
        var newRatio = (float) count / stageData.islandCountByColor[color];

        paletteButton.ColoredRatio = newRatio;

        // 다 칠한 팔레트 버튼은 사라진다.
        var completed = newRatio >= 1.0f;
        paletteButton.gameObject.SetActive(completed == false);

        // 사라지는 팔레트가 체크되어 있다면 그 다음 것으로 선택되게 한다.
        // 다음 것이 없으면 앞의 것을
        if (completed)
        {
            EnsurePaletteCheck(paletteIndex);
        }

        if (oldRatio >= 1.0f || newRatio < 1.0f) return;

        if (batch) return;

        if (poofPrefab == null) return;

        // 이번에 칠해서 사라졌다. 펑 효과 보여주자.
        var poof = Instantiate(poofPrefab, GetComponentInParent<Canvas>().transform).GetComponent<Poof>();
        var poofTransform = poof.transform;
        poofTransform.position = paletteButton.transform.position;
        poofTransform.localScale = Vector3.one;

        Sound.instance.PlayCorrectlyFinishedMild();
    }

    void EnsurePaletteCheck(int paletteIndex)
    {
        if (paletteIndex < 0 || paletteIndex >= paletteButtonList.Count)
        {
            return;
        }

        if (paletteButtonList[paletteIndex].Check == false)
        {
            return;
        }
        
        for (var i = paletteIndex + 1; i < paletteButtonList.Count; i++)
        {
            if (!paletteButtonList[i].gameObject.activeSelf) continue;
                
            paletteButtonList[i].Check = true;
            return;
        }
        
        for (var i = paletteIndex - 1; i >= 0; i--)
        {
            if (!paletteButtonList[i].gameObject.activeSelf) continue;
                
            paletteButtonList[i].Check = true;
            return;
        }
    }
}