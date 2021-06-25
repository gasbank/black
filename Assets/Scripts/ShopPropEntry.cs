using UnityEngine;
using UnityEngine.UI;

public class ShopPropEntry : MonoBehaviour {
    [SerializeField] Text propName;
    [SerializeField] Text propPrice;
    [SerializeField] GameObject propTarget;

    public GameObject PropTarget { get => propTarget; set => propTarget = value; }
    public bool PropTargetActive { get => propTarget.activeSelf; set => propTarget.SetActive(value); }
    public string PropName { get => propName.text; set => propName.text = value; }
    public string PropPrice { get => propPrice.text; set => propPrice.text = value; }
}
