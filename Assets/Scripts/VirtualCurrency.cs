using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirtualCurrency : MonoBehaviour
{
    public enum Type
    {
        Mango,
        Medal,
        Star
    }

    [SerializeField]
    Sprite[] currencySprites;

    [SerializeField]
    Type currencyType = 0;

    [SerializeField]
    int currencyValue;

    [SerializeField]
    Image image;

    [SerializeField]
    TextMeshProUGUI text;

    public int CurrencyValue
    {
        get => currencyValue;
        set
        {
            currencyValue = value;
            text.text = currencyValue.ToString();
        }
    }

    public Type CurrencyType
    {
        get => currencyType;
        set
        {
            currencyType = value;
            UpdateCurrencyTypeDependents();
        }
    }

    void UpdateCurrencyTypeDependents()
    {
        image.sprite = currencySprites[(int) currencyType];
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateCurrencyTypeDependents();
    }
#endif

    void Awake()
    {
        CurrencyValue = 0;
    }
}