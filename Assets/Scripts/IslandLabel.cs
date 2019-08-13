using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandLabel : MonoBehaviour {
    [SerializeField] Image backgroundImage = null;
    [SerializeField] TMPro.TextMeshProUGUI text = null;
    [SerializeField] RectTransform rt = null;

    public RectTransform Rt => rt;

    public string Text {
        get => text.text;
        set => text.text = value;
    }

    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }
}
