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

    Miniroom miniroomController;

    void AddAllProps()
    {
        if (miniroomController == null)
        {
            Debug.LogError("ShopPopup miniroom is not assigned or does not have a Miniroom component.", gameObject);
            return;
        }

        foreach (var relativePath in miniroomController.GetLeafPathList())
        {
            var entry = Instantiate(shopPropEntry, shopPropEntryParent).GetComponent<ShopPropEntry>();
            entry.PropName = relativePath.Split('/').Last();
            entry.Bind(miniroomController, relativePath);
        }
    }

    void Awake()
    {
        miniroomController = miniroom != null ? miniroom.GetComponent<Miniroom>() : null;
        AddAllProps();
    }

    void OpenPopup()
    {
        
    }

    void ClosePopup()
    {
        
    }
}
