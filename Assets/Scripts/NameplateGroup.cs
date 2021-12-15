using TMPro;
using UnityEngine;

public class NameplateGroup : MonoBehaviour
{
    [SerializeField]
    [AutoBindThis]
    CanvasGroup canvasGroup;

    [SerializeField]
    TextMeshProUGUI text;

    public string Text
    {
        set => text.text = value;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Awake()
    {
        canvasGroup.alpha = 0;
    }
}