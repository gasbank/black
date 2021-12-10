using MessagePack;
using MessagePack.Formatters;
using System.Buffers;

public sealed class BlackStringTableFormatter : IMessagePackFormatter<ScString> {
    public static readonly BlackStringTableFormatter Instance = new BlackStringTableFormatter();

    public void Serialize(ref MessagePackWriter writer, ScString value, MessagePackSerializerOptions options) {
        WriteStringBytes(ref writer, value.value);
    }

    public ScString Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
        var strBytes = ReadStringBytes(ref reader);
        return strBytes != null ? new ScString(strBytes) : null;
    }

    static void WriteStringBytes(ref MessagePackWriter writer, byte[] value) {
        if (value == null) {
            writer.WriteNil();
            return;
        }
        
        writer.Write(value);
    }

    static byte[] ReadStringBytes(ref MessagePackReader reader) {
        if (reader.IsNil) {
            reader.Skip();
            return null;
        }

        return reader.ReadBytes()?.ToArray();
    }
}
