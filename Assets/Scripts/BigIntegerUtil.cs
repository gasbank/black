using System.Numerics;
using Dirichlet.Numerics;

public static class BigIntegerUtil
{
    public static int ToClampedInt(this BigInteger bigInt)
    {
        if (bigInt < int.MinValue)
            return int.MinValue;
        if (bigInt > int.MaxValue) return int.MaxValue;
        return (int) bigInt;
    }

    public static long ToClampedLong(this BigInteger bigInt)
    {
        if (bigInt < long.MinValue)
            return long.MinValue;
        if (bigInt > long.MaxValue) return long.MaxValue;
        return (long) bigInt;
    }

    public static long ToClampedLong(this UInt128 bigInt)
    {
        if (bigInt < long.MinValue)
            return long.MinValue;
        if (bigInt > long.MaxValue) return long.MaxValue;
        return (long) bigInt;
    }

    public static uint ToClampedUInt(this UInt128 bigInt)
    {
        if (bigInt < uint.MinValue)
            return uint.MinValue;
        if (bigInt > uint.MaxValue) return uint.MaxValue;
        return (uint) bigInt;
    }
}