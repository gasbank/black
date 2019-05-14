using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour {
    [SerializeField] GameObject check = null;
    [SerializeField] Image image = null;

    public bool Check {
        get => check.activeSelf;
        set {
            foreach (Transform t in transform.parent) {
                var pb = t.GetComponent<PaletteButton>();
                if (pb != null) {
                    pb.check.SetActive(t == transform);
                }
            }
        }
    }

    public Color PaletteColor => image.color;
}
