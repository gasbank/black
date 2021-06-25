using UnityEngine;

public static class LayerMaskExtension
{
    public static bool Contains(this LayerMask layerMask, GameObject go) => Contains(layerMask, go.layer);
    public static bool Contains(this LayerMask layerMask, int layer) => ((1 << layer) & layerMask) != 0;
}
