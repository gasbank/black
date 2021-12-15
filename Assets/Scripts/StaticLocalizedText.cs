using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StaticLocalizedText : MonoBehaviour
{
    [SerializeField]
    string strRef = "";

    [SerializeField]
    Text text;

    public string StrRef => strRef;

    public static string ToLiteral(string input)
    {
        return input.Replace("\n", @"\n");
    }

    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = Data.dataSet != null ? FontManager.instance.ToLocalizedCurrent(this, "\\" + strRef) : strRef;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateStrRef();
    }

    void UpdateStrRef()
    {
        text = GetComponent<Text>();
        strRef = ToLiteral(text.text);
    }
#endif
}