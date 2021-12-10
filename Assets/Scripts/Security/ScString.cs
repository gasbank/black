using MessagePack;
using System;
using System.ComponentModel;
using System.Text;

public class ScStringConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
        return sourceType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
        return (ScString)(value as string);
    }
}

[TypeConverter(typeof(ScStringConverter))]
[Serializable]
[MessagePackObject]
public class ScString : IFormattable, IEquatable<ScString> {
    [IgnoreMember] public static readonly byte[] k = { 0x11, 0x5A, 0xCF, 0x5B };
    [Key(0)] public byte[] value;

    static bool ByteArrayCompare(byte[] a1, byte[] a2) {
        if (a1 == a2)
            return true;
        if (a1 == null && a2 != null)
            return false;
        if (a1 != null && a2 == null)
            return false;

        if (a1.Length != a2.Length)
            return false;

        for (int i = 0; i < a1.Length; i++)
            if (a1[i] != a2[i])
                return false;

        return true;
    }

    public ScString(byte[] value) {
        this.value = value;
    }

    public ScString(string value) {
        this.value = Encoding.UTF8.GetBytes(value);
        for (int i = 0; i < this.value.Length; i++) {
            this.value[i] ^= k[i % k.Length];
        }
    }

    // Implicit conversion from string to ScString.
    public static implicit operator ScString(string x) { return new ScString(x); }

    public static bool operator ==(ScString x, ScString y) { return ByteArrayCompare(x?.value ?? null, y?.value ?? null); }
    public static bool operator !=(ScString x, ScString y) { return !(x == y); }

    // Overload the conversion from ScString to string:
    public static implicit operator string(ScString x) {
        if (x == null) {
            return null;
        }
        byte[] xClone = (byte[])x.value.Clone();
        for (int i = 0; i < x.value.Length; i++) {
            xClone[i] ^= k[i % k.Length];
        }
        return Encoding.UTF8.GetString(xClone);
    }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o) {
        try {
            return ByteArrayCompare(value, ((ScString)o).value);
        } catch {
            return false;
        }
    }

    // Override the Object.GetHashCode() method:
    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    // Override the ToString method to convert DBBool to a string:
    public override string ToString() { return this; }

    public string ToString(string format, IFormatProvider formatProvider) {
        return ToString();
    }

    // IEquatable<T>를 만들면 Dictionary에서 TryGetValue 등으로 쿼리할 때
    // 가비지 생성을 생략할 수 있다. 굳...
    public bool Equals(ScString other) {
        if (this == other) {
            return true;
        }
        if (value == other.value) {
            return true;
        }
        return ByteArrayCompare(value, other.value);
    }
}
