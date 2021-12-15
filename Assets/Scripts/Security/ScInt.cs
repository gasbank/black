using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using MessagePack;

public class ScIntConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        return (ScInt) int.Parse(value as string);
    }
}

[TypeConverter(typeof(ScIntConverter))]
[Serializable]
[MessagePackObject]
public struct ScInt : IComparable<ScInt>, IFormattable, IEquatable<ScInt>
{
    [IgnoreMember]
    public static readonly int k = 0x2ABB0506;

    [Key(0)]
    public int value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ScInt(int value)
    {
        this.value = value ^ k;
    }

    // Implicit conversion from int to ScInt.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ScInt(int x)
    {
        return new ScInt(x);
    }

    // Implicit conversion from ScInt to int.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(ScInt x)
    {
        return x.value ^ k;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScInt operator ++(ScInt x)
    {
        x.value = ((ScInt) ((int) x + 1)).value;
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScInt operator --(ScInt x)
    {
        x.value = ((ScInt) ((int) x - 1)).value;
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ScInt x, ScInt y)
    {
        return x.value == y.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScInt operator +(ScInt x, ScInt y)
    {
        return (int) x + (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScInt operator -(ScInt x, ScInt y)
    {
        return (int) x - (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScInt operator *(ScInt x, ScInt y)
    {
        return (int) x * (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScInt operator /(ScInt x, ScInt y)
    {
        return (int) x / (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScInt operator %(ScInt x, ScInt y)
    {
        return (int) x % (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ScInt x, ScInt y)
    {
        return x.value != y.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(ScInt x, ScInt y)
    {
        return (int) x <= (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(ScInt x, ScInt y)
    {
        return (int) x >= (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(ScInt x, ScInt y)
    {
        return (int) x < (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(ScInt x, ScInt y)
    {
        return (int) x > (int) y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Overload the conversion from ScInt to string:
    public static implicit operator string(ScInt x)
    {
        return ((int) x).ToString();
    }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return value == ((ScInt) o).value;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt()
    {
        return this;
    }

    // Make ScInt as OrderBy-able using LINQ
    public int CompareTo(ScInt other)
    {
        return ((int) this).CompareTo(other);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return ((int) this).ToString(format, formatProvider);
    }

    // IEquatable<T>를 만들면 Dictionary에서 TryGetValue 등으로 쿼리할 때
    // 가비지 생성을 생략할 수 있다. 굳...
    public bool Equals(ScInt other)
    {
        return value == other.value;
    }
}