#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new System.Collections.Generic.Dictionary<Type, int>(8)
            {
                {typeof(System.Collections.Generic.HashSet<uint>), 0 },
                {typeof(ScInt), 1 },
                {typeof(GameSaveData), 2 },
                {typeof(StageSaveData), 3 },
                {typeof(ScBigInteger), 4 },
                {typeof(ScFloat), 5 },
                {typeof(ScLong), 6 },
                {typeof(ScString), 7 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new Formatters.HashSetFormatter<uint>();
                case 1: return new Formatters.ScIntFormatter();
                case 2: return new Formatters.GameSaveDataFormatter();
                case 3: return new Formatters.StageSaveDataFormatter();
                case 4: return new Formatters.ScBigIntegerFormatter();
                case 5: return new Formatters.ScFloatFormatter();
                case 6: return new Formatters.ScLongFormatter();
                case 7: return new Formatters.ScStringFormatter();
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


    public sealed class ScIntFormatter : IMessagePackFormatter<ScInt>
    {

        public int Serialize(ref byte[] bytes, int offset, ScInt value, IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.value);
            return offset - startOffset;
        }

        public ScInt Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
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
                        readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new ScInt(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class GameSaveDataFormatter : IMessagePackFormatter<GameSaveData>
    {

        public int Serialize(ref byte[] bytes, int offset, GameSaveData value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<ScInt>().Serialize(ref bytes, offset, value.gold, formatterResolver);
            return offset - startOffset;
        }

        public GameSaveData Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __gold__ = default(ScInt);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __gold__ = formatterResolver.GetFormatterWithVerify<ScInt>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new GameSaveData();
            ____result.gold = __gold__;
            return ____result;
        }
    }


    public sealed class StageSaveDataFormatter : IMessagePackFormatter<StageSaveData>
    {

        public int Serialize(ref byte[] bytes, int offset, StageSaveData value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.stageName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<System.Collections.Generic.HashSet<uint>>().Serialize(ref bytes, offset, value.coloredMinPoints, formatterResolver);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.zoomValue);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.targetImageCenterX);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.targetImageCenterY);
            return offset - startOffset;
        }

        public StageSaveData Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __stageName__ = default(string);
            var __coloredMinPoints__ = default(System.Collections.Generic.HashSet<uint>);
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
                        __coloredMinPoints__ = formatterResolver.GetFormatterWithVerify<System.Collections.Generic.HashSet<uint>>().Deserialize(bytes, offset, formatterResolver, out readSize);
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
                        readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new StageSaveData();
            ____result.stageName = __stageName__;
            ____result.coloredMinPoints = __coloredMinPoints__;
            ____result.zoomValue = __zoomValue__;
            ____result.targetImageCenterX = __targetImageCenterX__;
            ____result.targetImageCenterY = __targetImageCenterY__;
            return ____result;
        }
    }


    public sealed class ScBigIntegerFormatter : IMessagePackFormatter<ScBigInteger>
    {

        public int Serialize(ref byte[] bytes, int offset, ScBigInteger value, IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<System.Numerics.BigInteger>().Serialize(ref bytes, offset, value.value, formatterResolver);
            return offset - startOffset;
        }

        public ScBigInteger Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __value__ = default(System.Numerics.BigInteger);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __value__ = formatterResolver.GetFormatterWithVerify<System.Numerics.BigInteger>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new ScBigInteger(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class ScFloatFormatter : IMessagePackFormatter<ScFloat>
    {

        public int Serialize(ref byte[] bytes, int offset, ScFloat value, IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.value);
            return offset - startOffset;
        }

        public ScFloat Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
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
                        readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new ScFloat();
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class ScLongFormatter : IMessagePackFormatter<ScLong>
    {

        public int Serialize(ref byte[] bytes, int offset, ScLong value, IFormatterResolver formatterResolver)
        {
            
            var startOffset = offset;
            offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.value);
            return offset - startOffset;
        }

        public ScLong Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            var startOffset = offset;
            var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
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
                        readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new ScLong(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }


    public sealed class ScStringFormatter : IMessagePackFormatter<ScString>
    {

        public int Serialize(ref byte[] bytes, int offset, ScString value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.value, formatterResolver);
            return offset - startOffset;
        }

        public ScString Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
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
                        readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new ScString(__value__);
            ____result.value = __value__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
