using UnityEngine;
using UnityEngine.UI;

public class ShopPropEntry : MonoBehaviour
{
    [SerializeField]
    Text propName;

    [SerializeField]
    Text propPrice;

    [field: SerializeField]
    public GameObject PropTarget { get; set; }

    public bool PropTargetActive
    {
        get => PropTarget.GetComponent<CanvasGroupAlpha>().TargetAlpha > 0;
        set => PropTarget.GetComponent<CanvasGroupAlpha>().SetAlphaImmediately(value ? 1.0f : 0.0f);
    }

    public string PropName
    {
        get => propName.text;
        set => propName.text = value;
    }

    public string PropPrice
    {
        get => propPrice.text;
        set => propPrice.text = value;
    }
}