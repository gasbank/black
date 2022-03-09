using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Prop3DGroup : MonoBehaviour
{
    [SerializeField]
    Prop3D[] prop3DList;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Update()
    {
        var sortedProp3DArray = prop3DList.OrderByDescending(e => e.transform.position.z).ToArray();
        for (var i = 0; i < sortedProp3DArray.Length; i++)
        {
            sortedProp3DArray[i].SetSiblingIndexFor2D(i);
        }
    }
}