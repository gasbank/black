using UnityEngine;

public class ShopPopup : MonoBehaviour {
    [SerializeField] GameObject shopPropEntry = null;
    [SerializeField] Transform miniroom = null;
    [SerializeField] Transform shopPropEntryParent = null;

    private void AddAllProps() {
        var transformList = miniroom.GetComponentsInChildren<Transform>(true);
        foreach (var t in transformList) {
            var entry = Instantiate(shopPropEntry, shopPropEntryParent).GetComponent<ShopPropEntry>();
            entry.PropName = t.name;
            entry.PropTarget = t.gameObject;
        }
    }

    private void Awake() {
        AddAllProps();
    }
}
