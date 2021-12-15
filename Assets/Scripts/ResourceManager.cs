using System.Collections.Generic;
using UnityEngine;
using UInt128 = Dirichlet.Numerics.UInt128;

[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour {

    public static ResourceManager instance;

    public int accountLevel => BlackContext.instance.LastClearedStageId;
    public int accountLevelExp // unused
        =>
            1;

    public UInt128 accountGem => BlackContext.instance.Gem;
    public UInt128 accountRiceRate => 0;

    public Dictionary<string, int> RedeemedCouponCode = new Dictionary<string, int>();
}
