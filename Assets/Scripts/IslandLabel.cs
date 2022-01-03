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

    [SerializeField]
    TextMeshProUGUI textMark;

    public RectTransform Rt => rt;

    public string Text
    {
        set
        {
            text.text = value;
            // ReSharper disable once StringLiteralTypo
            textMark.text = $"<mark=#ffffffff>{value}</mark>";
        }
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