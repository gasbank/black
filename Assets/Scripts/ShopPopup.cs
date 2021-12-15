using System.Linq;
using UnityEngine;

public class ShopPopup : MonoBehaviour
{
    [SerializeField]
    Transform miniroom;

    [SerializeField]
    GameObject shopPropEntry;

    [SerializeField]
    Transform shopPropEntryParent;

    void AddAllProps()
    {
        var leafTransformList = miniroom.GetComponentsInChildren<Transform>(true)
            .Where(e => e.transform.childCount == 0);

        foreach (var t in leafTransformList)
        {
            var entry = Instantiate(shopPropEntry, shopPropEntryParent).GetComponent<ShopPropEntry>();
            entry.PropName = t.name;
            entry.PropTarget = t.gameObject;
        }
    }

    void Awake()
    {
        AddAllProps();
    }
}