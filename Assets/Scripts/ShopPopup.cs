using UnityEngine;

public class ShopPopup : MonoBehaviour {
    [SerializeField] GameObject shopPropEntry;
    [SerializeField] Transform miniroom;
    [SerializeField] Transform shopPropEntryParent;

    void AddAllProps() {
        var transformList = miniroom.GetComponentsInChildren<Transform>(true);
        foreach (var t in transformList) {
            var entry = Instantiate(shopPropEntry, shopPropEntryParent).GetComponent<ShopPropEntry>();
            entry.PropName = t.name;
            entry.PropTarget = t.gameObject;
        }
    }

    void Awake() {
        AddAllProps();
    }
}
