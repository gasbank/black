using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Sort3DGroup : MonoBehaviour
{
    [SerializeField]
    Prop3D[] prop3DList;
    
    [SerializeField]
    Character3D[] char3DList;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Update()
    {
        var sortedProp3DArray = prop3DList
            .Cast<IWorldPosition3D>()
            .Concat(char3DList)
            .OrderByDescending(e => e.WorldPosition3D.z)
            .ThenByDescending(e => e.WorldPosition3D.x)
            .ThenBy(e => e.WorldPosition3D.y)
            .ToArray();
        for (var i = 0; i < sortedProp3DArray.Length; i++)
        {
            sortedProp3DArray[i].SetSiblingIndexFor2D(i);
        }
    }
}