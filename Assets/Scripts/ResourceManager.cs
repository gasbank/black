using System.Collections.Generic;
using Dirichlet.Numerics;
using UnityEngine;

[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance;

    public Dictionary<string, int> RedeemedCouponCode = new Dictionary<string, int>();

    public int accountLevel => BlackContext.instance.LastClearedStageId;

    public int accountLevelExp // unused
        =>
            1;

    public UInt128 accountGem => BlackContext.instance.Gem;
    public UInt128 accountGoldRate => 0;
}