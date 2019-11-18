using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public class ScIntConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {   
        return (ScInt)int.Parse(value as string);
    }
}

[TypeConverter(typeof(ScIntConverter))]
[Serializable]
[MessagePackObject]
public struct ScInt : IComparable<ScInt>, IFormattable
{
    // 10진수로는 787,284,511
    [IgnoreMember] public static readonly int k = 0x02920101;
    [Key(0)] public int value;

    public ScInt(int value) { this.value = value ^ k; }

    // Implicit conversion from int to ScInt.
    public static implicit operator ScInt(int x) { return new ScInt(x); }

    // Implicit conversion from ScInt to int.
    public static implicit operator int(ScInt x) { return x.value ^ k; }

    public static ScInt operator ++(ScInt x) { x.value = ((ScInt)((int)x + 1)).value; return x; }

    public static ScInt operator --(ScInt x) { x.value = ((ScInt)((int)x - 1)).value; return x; }

    public static bool operator ==(ScInt x, ScInt y) { return x.value == y.value; }

    public static ScInt operator +(ScInt x, ScInt y) { return ((int)x + (int)y); }

    public static ScInt operator -(ScInt x, ScInt y) { return ((int)x - (int)y); }

    public static ScInt operator *(ScInt x, ScInt y) { return ((int)x * (int)y); }

    public static ScInt operator /(ScInt x, ScInt y) { return ((int)x / (int)y); }

    public static ScInt operator %(ScInt x, ScInt y) { return ((int)x % (int)y); }

    public static bool operator !=(ScInt x, ScInt y) { return x.value != y.value; }

    public static bool operator <=(ScInt x, ScInt y) { return ((int)x <= (int)y); }

    public static bool operator >=(ScInt x, ScInt y) { return ((int)x >= (int)y); }

    public static bool operator <(ScInt x, ScInt y) { return ((int)x < (int)y); }

    public static bool operator >(ScInt x, ScInt y) { return ((int)x > (int)y); }

    // Overload the conversion from ScInt to string:
    public static implicit operator string(ScInt x) { return ((int)x).ToString(); }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return value == ((ScInt)o).value;
        }
        catch
        {
            return false;
        }
    }

    // Override the Object.GetHashCode() method:
    public override int GetHashCode() { return value.GetHashCode(); }

    // Override the ToString method to convert DBBool to a string:
    public override string ToString() { return this; }

    public int ToInt() { return this; }

    // Make ScInt as OrderBy-able using LINQ
    public int CompareTo(ScInt other) { return ((int)this).CompareTo(other); }

    public string ToString(string format, IFormatProvider formatProvider) {
        return ((int)this).ToString(format, formatProvider);
    }
}

