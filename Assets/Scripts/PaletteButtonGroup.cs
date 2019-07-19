using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PaletteButtonGroup : MonoBehaviour {
    [SerializeField] PaletteButton paletteButtonPrefab = null;

    public Color CurrentPaletteColor {
        get {
            foreach (Transform t in transform) {
                var pb = t.GetComponent<PaletteButton>();
                if (pb.Check) {
                    return pb.PaletteColor;
                }
            }
            return Color.white;
        }
    }

    public void CreatePalette(StageData stageData) {
        DestroyAllPaletteButtons();

        var colorUintArray = stageData.islandDataByMinPoint.Select(e => e.Value.rgba).Distinct().OrderBy(e => e).ToArray();
        foreach (var colorUint in colorUintArray) {
            var paletteButton = Instantiate(paletteButtonPrefab, transform).GetComponent<PaletteButton>();
            paletteButton.PaletteColor = BlackConvert.GetColor(colorUint);
        }
    }

    private void DestroyAllPaletteButtons() {
        foreach (var t in transform.Cast<Transform>().ToArray()) {
            Destroy(t.gameObject);
        }
    }
}
