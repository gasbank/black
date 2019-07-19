using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour {
    [SerializeField] GameObject check = null;
    [SerializeField] Image image = null;
    [SerializeField] TMPro.TextMeshProUGUI colorNumberText = null;

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

    public Color PaletteColor {
        get => image.color;
        set => image.color = value;
    }

    public int ColorIndex {
        get => int.Parse(colorNumberText.text);
        set => colorNumberText.text = value.ToString();
    }
}
