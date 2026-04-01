using UnityEngine;
using UnityEngine.UI;

public class ShopPropEntry : MonoBehaviour
{
    [SerializeField]
    Text propName;

    [SerializeField]
    Text propPgold;

    [field: SerializeField]
    public GameObject PropTarget { get; private set; }

    Miniroom miniroom;
    string propRelativePath;

    public bool PropTargetActive
    {
        get => miniroom != null && miniroom.IsLeafVisible(propRelativePath);
        set
        {
            if (miniroom == null || string.IsNullOrEmpty(propRelativePath))
            {
                return;
            }

            miniroom.TrySetLeafVisibility(propRelativePath, value ? 1.0f : 0.0f);
        }
    }

    public string PropName
    {
        get => propName.text;
        set => propName.text = value;
    }

    public string PropPgold
    {
        get => propPgold.text;
        set => propPgold.text = value;
    }

    public void Bind(Miniroom targetMiniroom, string targetRelativePath)
    {
        miniroom = targetMiniroom;
        propRelativePath = targetRelativePath;
        PropTarget = null;
    }
}
