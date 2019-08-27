using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualCurrency : MonoBehaviour {
    [SerializeField] TMPro.TextMeshProUGUI text = null;
    [SerializeField] Image image = null;
    [SerializeField] int currencyValue = 0;
    [SerializeField] Type currencyType = 0;
    [SerializeField] Sprite[] currencySprites = null;

    public enum Type {
        Mango,
        Medal,
        Star,
    }

    public int CurrencyValue {
        get => currencyValue;
        set {
            currencyValue = value;
            text.text = currencyValue.ToString();
        }
    }

    public Type CurrencyType {
        get => currencyType;
        set {
            currencyType = value;
            UpdateCurrencyTypeDependents();
        }
    }

    private void UpdateCurrencyTypeDependents() {
        image.sprite = currencySprites[(int)currencyType];
    }

    void OnValidate() {
        UpdateCurrencyTypeDependents();
    }

    void Awake() {
        CurrencyValue = 0;
    }
}
