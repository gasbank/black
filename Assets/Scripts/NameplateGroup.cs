using TMPro;
using UnityEngine;

public class NameplateGroup : MonoBehaviour {
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField, AutoBindThis]
    CanvasGroup canvasGroup;

#if UNITY_EDITOR
    void OnValidate() {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Awake() {
        canvasGroup.alpha = 0;
    }

    public string Text {
        set => text.text = value;
    }
}