using System;
using System.ComponentModel;
using System.Globalization;
using MessagePack;

public class ScLongConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        var v = value as string;
        if (v.ToUpperInvariant().Contains("E+"))
        {
            var vd = double.Parse(v);
            if (vd >= long.MinValue && vd <= long.MaxValue)
                return (ScLong) (long) vd;
            throw new ArgumentOutOfRangeException();
        }

        return (ScLong) long.Parse(v);
    }
}

[TypeConverter(typeof(ScLongConverter))]
[Serializable]
[MessagePackObject]
public struct ScLong : IFormattable, IEquatable<ScLong>, IComparable<ScLong>
{
    // 10진수로는 8,432,152,544,025,910,249
    [IgnoreMember]
    public static readonly long k = 0x7505061036921BE9;

    [Key(0)]
    public long value;

    public ScLong(long value)
    {
        this.value = value ^ k;
    }

    // Implicit conversion from long to ScLong.
    public static implicit operator ScLong(long x)
    {
        return new ScLong(x);
    }

    // Explicit conversion from ScLong to long.
    public static implicit operator long(ScLong x)
    {
        return x.value ^ k;
    }

    public static ScLong operator ++(ScLong x)
    {
        x.value = ((ScLong) ((long) x + 1)).value;
        return x;
    }

    public static ScLong operator --(ScLong x)
    {
        x.value = ((ScLong) ((long) x - 1)).value;
        return x;
    }

    public static bool operator ==(ScLong x, ScLong y)
    {
        return x.value == y.value;
    }

    public static ScLong operator +(ScLong x, ScLong y)
    {
        return (long) x + (long) y;
    }

    public static ScLong operator -(ScLong x, ScLong y)
    {
        return (long) x - (long) y;
    }

    public static ScLong operator *(ScLong x, ScLong y)
    {
        return (long) x * (long) y;
    }

    public static ScLong operator /(ScLong x, ScLong y)
    {
        return (long) x / (long) y;
    }

    public static ScLong operator %(ScLong x, ScLong y)
    {
        return (long) x % (long) y;
    }

    public static bool operator !=(ScLong x, ScLong y)
    {
        return x.value != y.value;
    }

    public static bool operator <=(ScLong x, ScLong y)
    {
        return (long) x <= (long) y;
    }

    public static bool operator >=(ScLong x, ScLong y)
    {
        return (long) x >= (long) y;
    }

    public static bool operator <(ScLong x, ScLong y)
    {
        return (long) x < (long) y;
    }

    public static bool operator >(ScLong x, ScLong y)
    {
        return (long) x > (long) y;
    }

    // Overload the conversion from ScLong to string:
    public static implicit operator string(ScLong x)
    {
        return ((long) x).ToString();
    }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return value == ((ScLong) o).value;
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

    // Override the ToString method to convert DBBool to a string:
    public string ToString(string format)
    {
        return ToLong().ToString(format);
    }

    public long ToLong()
    {
        return this;
    }

    // Make ScLong as OrderBy-able using LINQ
    public int CompareTo(ScLong other)
    {
        return ((long) this).CompareTo(other);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return ToLong().ToString(format, formatProvider);
    }

    // IEquatable<T>를 만들면 Dictionary에서 TryGetValue 등으로 쿼리할 때
    // 가비지 생성을 생략할 수 있다. 굳...
    public bool Equals(ScLong other)
    {
        return value == other.value;
    }
}