using MessagePack;
using System;
using System.ComponentModel;

public class ScBigIntegerConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
        return (ScBigInteger)System.Numerics.BigInteger.Parse(value as string ?? throw new InvalidCastException());
    }
}

[TypeConverter(typeof(ScBigIntegerConverter))]
[Serializable]
[MessagePackObject]
public struct ScBigInteger {
    [IgnoreMember] public static readonly System.Numerics.BigInteger k = 0x12344321ABCDDCBA;
    [Key(0)] public System.Numerics.BigInteger value;

    public ScBigInteger(System.Numerics.BigInteger value) { this.value = value ^ k; }
    public ScBigInteger(string value) { this.value = System.Numerics.BigInteger.Parse(value) ^ k; }

    // Implicit conversion from BigInteger to ScBigInteger.
    public static implicit operator ScBigInteger(System.Numerics.BigInteger x) { return new ScBigInteger(x); }
    // Implicit conversion from long to ScBigInteger.
    public static implicit operator ScBigInteger(long x) { return new ScBigInteger(x); }

    // Explicit conversion from ScBigInteger to BigInteger.
    public static implicit operator System.Numerics.BigInteger(ScBigInteger x) { return x.value ^ k; }

    public static ScBigInteger operator ++(ScBigInteger x) { x.value = ((ScBigInteger)((System.Numerics.BigInteger)x + 1)).value; return x; }

    public static ScBigInteger operator --(ScBigInteger x) { x.value = ((ScBigInteger)((System.Numerics.BigInteger)x - 1)).value; return x; }

    public static bool operator ==(ScBigInteger x, ScBigInteger y) { return x.value == y.value; }

    public static ScBigInteger operator +(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x + (System.Numerics.BigInteger)y); }

    public static ScBigInteger operator -(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x - (System.Numerics.BigInteger)y); }

    public static ScBigInteger operator *(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x * (System.Numerics.BigInteger)y); }

    public static ScBigInteger operator /(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x / (System.Numerics.BigInteger)y); }

    public static ScBigInteger operator %(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x % (System.Numerics.BigInteger)y); }

    public static bool operator !=(ScBigInteger x, ScBigInteger y) { return x.value != y.value; }

    public static bool operator <=(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x <= (System.Numerics.BigInteger)y); }

    public static bool operator >=(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x >= (System.Numerics.BigInteger)y); }

    public static bool operator <(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x < (System.Numerics.BigInteger)y); }

    public static bool operator >(ScBigInteger x, ScBigInteger y) { return ((System.Numerics.BigInteger)x > (System.Numerics.BigInteger)y); }

    // Overload the conversion from ScBigInteger to string:
    public static implicit operator string(ScBigInteger x) { return ((System.Numerics.BigInteger)x).ToString(); }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o) {
        try {
            return value == ((ScBigInteger)o).value;
        } catch {
            return false;
        }
    }

    // Override the Object.GetHashCode() method:
    public override int GetHashCode() { return value.GetHashCode(); }

    // Override the ToString method to convert DBBool to a string:
    public override string ToString() { return this; }

    public System.Numerics.BigInteger ToBigInteger() { return this; }

    public Dirichlet.Numerics.UInt128 ToUInt128() { return UInt128BigInteger.FromBigInteger(ToBigInteger()); }

    // Make ScBigInteger as OrderBy-able using LINQ
    public int CompareTo(ScBigInteger other) { return ((System.Numerics.BigInteger)this).CompareTo(other); }
}
