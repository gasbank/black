using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Sort3DGroup : MonoBehaviour
{
    [SerializeField]
    List<Prop3D> prop3DList;

    [SerializeField]
    List<Character3D> char3DList;

    readonly List<IWorldPosition3D> runtimeList = new List<IWorldPosition3D>();

    public int RuntimeListCount => runtimeList.Count;

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
            .Concat(runtimeList)
            .OrderByDescending(e => e.WorldPosition3D.z)
            .ThenByDescending(e => e.WorldPosition3D.x)
            .ThenBy(e => e.WorldPosition3D.y)
            .ToArray();
        for (var i = 0; i < sortedProp3DArray.Length; i++)
        {
            sortedProp3DArray[i].SetSiblingIndexFor2D(i);
        }
    }

    public void Add(IWorldPosition3D worldPosition3D)
    {
        runtimeList.Add(worldPosition3D);
    }

    public void Remove(IWorldPosition3D worldPosition3D)
    {
        runtimeList.Remove(worldPosition3D);
    }
}