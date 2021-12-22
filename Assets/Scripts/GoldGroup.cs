using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GoldGroup : MonoBehaviour
{
    [SerializeField]
    Text goldText;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        UpdateGold();
    }

    void OnEnable()
    {
        BlackContext.instance.OnGoldChanged += UpdateGold;
    }

    void OnDisable()
    {
        BlackContext.instance.OnGoldChanged -= UpdateGold;
    }

    void UpdateGold()
    {
        goldText.text = BlackContext.instance.Gold.ToString();
    }
}
