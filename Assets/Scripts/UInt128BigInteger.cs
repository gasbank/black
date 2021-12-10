using Dirichlet.Numerics;

static public class UInt128BigInteger {
    public static System.Numerics.BigInteger ToBigInteger(UInt128 v) {
        if (v.s1 == 0)
            return v.s0;
        return (System.Numerics.BigInteger)v.s1 << 64 | v.s0;
    }
    
    public static UInt128 FromBigInteger(System.Numerics.BigInteger a) {
        return new UInt128 {s0 = (ulong)(a & ulong.MaxValue), s1 = (ulong)(a >> 64)};
    }
}