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
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(8)
            {
                {typeof(global::System.Collections.Generic.HashSet<uint>), 0 },
                {typeof(global::ScInt), 1 },
                {typeof(global::GameSaveData), 2 },
                {typeof(global::StageSaveData), 3 },
                {typeof(global::ScBigInteger), 4 },
                {typeof(global::ScFloat), 5 },
                {typeof(global::ScLong), 6 },
                {typeof(global::ScString), 7 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.HashSetFormatter<uint>();
                case 1: return new MessagePack.Formatters.ScIntFormatter();
                case 2: return new MessagePack.Formatters.GameSaveDataFormatter();
                case 3: return new MessagePack.Formatters.StageSaveDataFormatter();
                case 4: return new MessagePack.Formatters.ScBigIntegerFormatter();
                case 5: return new MessagePack.Formatters.ScFloatFormatter();
                case 6: return new MessagePack.Formatters.ScLongFormatter();
                case 7: return new MessagePack.Formatters.ScStringFormatter();
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


    public sealed class ScIntFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ScInt>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ScInt value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.value);
            return offset - startOffset;
        }

        public global::ScInt Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __value__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __value__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ScInt(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class GameSaveDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::GameSaveData>
    {

        public int Serialize(ref byte[] bytes, int offset, global::GameSaveData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::ScInt>().Serialize(ref bytes, offset, value.gold, formatterResolver);
            return offset - startOffset;
        }

        public global::GameSaveData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __gold__ = default(global::ScInt);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __gold__ = formatterResolver.GetFormatterWithVerify<global::ScInt>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::GameSaveData();
            ____result.gold = __gold__;
            return ____result;
        }
    }


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


    public sealed class ScBigIntegerFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ScBigInteger>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ScBigInteger value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Numerics.BigInteger>().Serialize(ref bytes, offset, value.value, formatterResolver);
            return offset - startOffset;
        }

        public global::ScBigInteger Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __value__ = default(global::System.Numerics.BigInteger);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __value__ = formatterResolver.GetFormatterWithVerify<global::System.Numerics.BigInteger>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ScBigInteger(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class ScFloatFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ScFloat>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ScFloat value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.value);
            return offset - startOffset;
        }

        public global::ScFloat Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __value__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __value__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ScFloat();
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class ScLongFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ScLong>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ScLong value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.value);
            return offset - startOffset;
        }

        public global::ScLong Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __value__ = default(long);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __value__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ScLong(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class ScStringFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ScString>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ScString value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.value, formatterResolver);
            return offset - startOffset;
        }

        public global::ScString Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __value__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __value__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ScString(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
