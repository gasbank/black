using System.Collections.Generic;
using System.Linq;
using ConditionalDebug;
using UnityEngine;

public class PaletteButtonGroup : MonoBehaviour
{
    [SerializeField]
    List<PaletteButton> paletteButtonList;

    [SerializeField]
    PaletteButton paletteButtonPrefab;

    readonly Dictionary<uint, int> paletteIndexbyColor = new Dictionary<uint, int>();

    StageData stageData;

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
                    ConDebug.Log($"CurrentPaletteColorUint: {pb.ColorUint} (0x{pb.ColorUint:X8})");
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

    public void UpdateColoredCount(uint color, int count)
    {
        paletteButtonList[GetPaletteIndexByColor(color)].ColoredRatio =
            (float) count / stageData.islandCountByColor[color];
    }
}