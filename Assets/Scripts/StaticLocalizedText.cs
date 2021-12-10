using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StaticLocalizedText : MonoBehaviour {
    [SerializeField]
    Text text;
    [SerializeField]
    string strRef = "";
    public string StrRef { get { return strRef; } }

    public static string ToLiteral(string input) {
        return input.Replace("\n", @"\n");
    }

#if UNITY_EDITOR
    void OnValidate() {
        UpdateStrRef();
    }

    void UpdateStrRef() {
        text = GetComponent<Text>();
        strRef = ToLiteral(text.text);
    }
#endif

    void Start() {
        UpdateText();
    }

    public void UpdateText() {
        text.text = Data.dataSet != null ? FontManager.instance.ToLocalizedCurrent(this, "\\" + strRef) : strRef;
    }
}
