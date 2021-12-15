using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TextCollector : MonoBehaviour
{
    [SerializeField]
    StaticLocalizedText[] allStaticLocalizedTextsInPrefab;

    [SerializeField]
    Text[] allTextsInPrefab;

    public Text[] AllTextsInPrefab => allTextsInPrefab;
    public StaticLocalizedText[] AllStaticLocalizedTextsInPrefab => allStaticLocalizedTextsInPrefab;

#if UNITY_EDITOR
    //모든 prefab의 prefabText 읽어오기
    public void ReadAllTextInPrefab()
    {
        allTextsInPrefab = transform.GetComponentsInChildren<Text>(true);
        allStaticLocalizedTextsInPrefab = transform.GetComponentsInChildren<StaticLocalizedText>(true);
    }

    void OnValidate()
    {
        ReadAllTextInPrefab();
    }
#endif
}