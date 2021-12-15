using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class TextCopier : MonoBehaviour
{
    [SerializeField]
    Text originalText;

    [SerializeField]
    Text text;

#if UNITY_EDITOR
    void OnValidate()
    {
        text = GetComponent<Text>();
    }
#endif

    void LateUpdate()
    {
        if (text != null && originalText != null) text.text = originalText.text;
    }
}