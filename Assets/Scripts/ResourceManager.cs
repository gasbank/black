using System.Collections.Generic;
using UnityEngine;
using UInt128 = Dirichlet.Numerics.UInt128;

[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour {

    public static ResourceManager instance;

    public int accountLevel { get { return BlackContext.instance.MahjongLastClearedStageId; } }
    public int accountLevelExp { get { return 1; } } // unused
    public UInt128 accountGem { get { return BlackContext.instance.Gem; } }
    public UInt128 accountRiceRate { get { return 0; } }

    public Dictionary<string, int> RedeemedCouponCode = new Dictionary<string, int>();
}
