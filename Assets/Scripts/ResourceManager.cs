using System.Collections.Generic;
using Dirichlet.Numerics;
using UnityEngine;

[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public Dictionary<string, int> RedeemedCouponCode = new Dictionary<string, int>();

    public int accountLevel => BlackContext.Instance.LastClearedStageId;

    public int accountLevelExp // unused
        =>
            1;

    public UInt128 accountGem => BlackContext.Instance.Gem;
    public UInt128 accountGoldRate => 0;
}