using UnityEngine;
using UnityEngine.UI;

public class ShopPropEntry : MonoBehaviour {
    [SerializeField] Text propName = null;
    [SerializeField] Text propPrice = null;
    [SerializeField] GameObject propTarget = null;

    public GameObject PropTarget { get => propTarget; set => propTarget = value; }
    public bool PropTargetActive { get => propTarget.activeSelf; set => propTarget.SetActive(value); }
    public string PropName { get => propName.text; set => propName.text = value; }
    public string PropPrice { get => propPrice.text; set => propPrice.text = value; }
}
