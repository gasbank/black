using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandLabel : MonoBehaviour {
    [SerializeField] Image backgroundImage;
    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] RectTransform rt;

    public RectTransform Rt => rt;

    public string Text {
        get => text.text;
        set => text.text = value;
    }

    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }

    public bool BackgroundImageActive {
        get => backgroundImage.gameObject.activeSelf;
        set => backgroundImage.gameObject.SetActive(value);
    }
}
