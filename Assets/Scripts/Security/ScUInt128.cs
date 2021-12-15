using System;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using Dirichlet.Numerics;
using MessagePack;

public class ScUInt128Converter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (!(value is string v)) throw new NullReferenceException();

        if (!v.ToUpperInvariant().Contains("E+")) return (ScUInt128) UInt128.Parse(v);

        var vd = BigInteger.Parse(v);
        if (vd >= 0 && vd <= UInt128BigInteger.ToBigInteger(UInt128.MaxValue))
            return (ScUInt128) UInt128BigInteger.FromBigInteger(vd);

        throw new ArgumentOutOfRangeException();
    }
}

[TypeConverter(typeof(ScUInt128Converter))]
[Serializable]
[MessagePackObject]
public struct ScUInt128 : IFormattable, IEquatable<ScUInt128>
{
    [IgnoreMember]
    public static readonly ulong k0 = 0x6010239384774891;

    [IgnoreMember]
    public static readonly ulong k1 = 0x1928202000190488;

    [Key(0)]
    public UInt128 value;

    public ScUInt128(UInt128 value)
    {
        UInt128.Create(out this.value, value.S0 ^ k0, value.S1 ^ k1);
    }

    public ScUInt128(ulong value)
    {
        UInt128.Create(out this.value, value ^ k0, k1);
    }
//    public ScUInt128(System.Numerics.BigInteger value) {
//        UInt128.Create(out this.value, value);
//        UInt128.Create(out this.value, this.value.S0 ^ k0, this.value.S1 ^ k1);
//    }

    public ScUInt128(string value)
    {
        this.value = ((ScUInt128) UInt128.Parse(value)).value;
    }

    // Implicit conversion from UInt128 to ScUInt128.
    public static implicit operator ScUInt128(UInt128 x)
    {
        return new ScUInt128(x);
    }

    public static implicit operator ScUInt128(ulong x)
    {
        return new ScUInt128(x);
    }
    //public static implicit operator ScUInt128(System.Numerics.BigInteger x) { return new ScUInt128(x); }
    //public static implicit operator ScUInt128(ScBigInteger x) { return new ScUInt128(x.ToBigInteger()); }

    // Explicit conversion from ScUInt128 to UInt128.
    public static implicit operator UInt128(ScUInt128 x)
    {
        return UInt128.Create(x.value.S0 ^ k0, x.value.S1 ^ k1);
    }

    public static ScUInt128 operator ++(ScUInt128 x)
    {
        x.value = ((ScUInt128) ((UInt128) x + 1)).value;
        return x;
    }

    public static ScUInt128 operator --(ScUInt128 x)
    {
        x.value = ((ScUInt128) ((UInt128) x - 1)).value;
        return x;
    }

    public static bool operator ==(ScUInt128 x, ScUInt128 y)
    {
        return x.value == y.value;
    }

    public static ScUInt128 operator +(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x + (UInt128) y;
    }


    public static ScUInt128 operator -(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x - (UInt128) y;
    }

    public static ScUInt128 operator *(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x * (UInt128) y;
    }

    public static ScUInt128 operator /(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x / (UInt128) y;
    }

    public static ScUInt128 operator %(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x % (UInt128) y;
    }

    public static bool operator !=(ScUInt128 x, ScUInt128 y)
    {
        return x.value != y.value;
    }

    public static bool operator <=(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x <= (UInt128) y;
    }

    public static bool operator >=(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x >= (UInt128) y;
    }

    public static bool operator <(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x < (UInt128) y;
    }

    public static bool operator >(ScUInt128 x, ScUInt128 y)
    {
        return (UInt128) x > (UInt128) y;
    }

    // Overload the conversion from ScUInt128 to string:
    public static implicit operator string(ScUInt128 x)
    {
        return ((UInt128) x).ToString();
    }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return value == ((ScUInt128) o).value;
        }
        catch
        {
            return false;
        }
    }

    // Override the Object.GetHashCode() method:
    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    // Override the ToString method to convert DBBool to a string:
    public override string ToString()
    {
        return this;
    }

    public UInt128 ToUInt128()
    {
        return this;
    }

    //public System.Numerics.BigInteger ToBigInteger() { return ((UInt128)this).ToBigInteger(); }

    // Make ScUInt128 as OrderBy-able using LINQ
    public int CompareTo(ScUInt128 other)
    {
        return ((UInt128) this).CompareTo(other);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return ToUInt128().ToString(format, formatProvider);
    }

    // IEquatable<T>를 만들면 Dictionary에서 TryGetValue 등으로 쿼리할 때
    // 가비지 생성을 생략할 수 있다. 굳...
    public bool Equals(ScUInt128 other)
    {
        return value == other.value;
    }
}