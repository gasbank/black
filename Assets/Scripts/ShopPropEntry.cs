using UnityEngine;
using UnityEngine.UI;

public class ShopPropEntry : MonoBehaviour {
    [SerializeField]
    Text propName;

    [SerializeField]
    Text propPrice;

    [SerializeField]
    GameObject propTarget;

    public GameObject PropTarget {
        get => propTarget;
        set => propTarget = value;
    }

    public bool PropTargetActive {
        get => propTarget.GetComponent<CanvasGroupAlpha>().TargetAlpha > 0;
        set => propTarget.GetComponent<CanvasGroupAlpha>().SetAlphaImmediately(value ? 1.0f : 0.0f);
    }

    public string PropName {
        get => propName.text;
        set => propName.text = value;
    }

    public string PropPrice {
        get => propPrice.text;
        set => propPrice.text = value;
    }
}