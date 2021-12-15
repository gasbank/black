using System.Numerics;
using Dirichlet.Numerics;

public static class UInt128BigInteger
{
    public static BigInteger ToBigInteger(UInt128 v)
    {
        if (v.s1 == 0)
            return v.s0;
        return ((BigInteger) v.s1 << 64) | v.s0;
    }

    public static UInt128 FromBigInteger(BigInteger a)
    {
        return new UInt128 {s0 = (ulong) (a & ulong.MaxValue), s1 = (ulong) (a >> 64)};
    }
}