using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IslandLabel : MonoBehaviour
{
    [SerializeField]
    Image backgroundImage;

    [SerializeField]
    RectTransform rt;

    [SerializeField]
    TextMeshProUGUI text;

    public RectTransform Rt => rt;

    public string Text
    {
        get => text.text;
        set => text.text = value;
    }

    public bool BackgroundImageActive
    {
        get => backgroundImage.gameObject.activeSelf;
        set => backgroundImage.gameObject.SetActive(value);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
#endif
}