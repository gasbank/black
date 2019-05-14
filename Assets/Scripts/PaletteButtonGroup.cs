using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteButtonGroup : MonoBehaviour {
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
}
