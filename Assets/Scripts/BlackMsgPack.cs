#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(2)
            {
                {typeof(global::System.Collections.Generic.HashSet<uint>), 0 },
                {typeof(global::StageSaveData), 1 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.HashSetFormatter<uint>();
                case 1: return new MessagePack.Formatters.StageSaveDataFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612



#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters
{
    using System;
    using MessagePack;


    public sealed class StageSaveDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::StageSaveData>
    {

        public int Serialize(ref byte[] bytes, int offset, global::StageSaveData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.stageName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.HashSet<uint>>().Serialize(ref bytes, offset, value.coloredMinPoints, formatterResolver);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.zoomValue);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.targetImageCenterX);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.targetImageCenterY);
            return offset - startOffset;
        }

        public global::StageSaveData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __stageName__ = default(string);
            var __coloredMinPoints__ = default(global::System.Collections.Generic.HashSet<uint>);
            var __zoomValue__ = default(float);
            var __targetImageCenterX__ = default(float);
            var __targetImageCenterY__ = default(float);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __stageName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __coloredMinPoints__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.HashSet<uint>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __zoomValue__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 3:
                        __targetImageCenterX__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 4:
                        __targetImageCenterY__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::StageSaveData();
            ____result.stageName = __stageName__;
            ____result.coloredMinPoints = __coloredMinPoints__;
            ____result.zoomValue = __zoomValue__;
            ____result.targetImageCenterX = __targetImageCenterX__;
            ____result.targetImageCenterY = __targetImageCenterY__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
