public static class BigIntegerUtil {
    public static int ToClampedInt(this System.Numerics.BigInteger bigInt) {
        if (bigInt < int.MinValue) {
            return int.MinValue;
        } else if (bigInt > int.MaxValue) {
            return int.MaxValue;
        }
        return (int)bigInt;
    }

    public static long ToClampedLong(this System.Numerics.BigInteger bigInt) {
        if (bigInt < long.MinValue) {
            return long.MinValue;
        } else if (bigInt > long.MaxValue) {
            return long.MaxValue;
        }
        return (long)bigInt;
    }

    public static long ToClampedLong(this Dirichlet.Numerics.UInt128 bigInt) {
        if (bigInt < long.MinValue) {
            return long.MinValue;
        } else if (bigInt > long.MaxValue) {
            return long.MaxValue;
        }
        return (long)bigInt;
    }

    public static uint ToClampedUInt(this Dirichlet.Numerics.UInt128 bigInt) {
        if (bigInt < uint.MinValue) {
            return uint.MinValue;
        } else if (bigInt > uint.MaxValue) {
            return uint.MaxValue;
        }
        return (uint)bigInt;
    }
}
