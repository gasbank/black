using System;
using MessagePack;
using MessagePack.Formatters;

public sealed class BlackStringResolver : IFormatterResolver
{
    public static readonly IFormatterResolver Instance = new BlackStringResolver();

    BlackStringResolver()
    {
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        return FormatterCache<T>.formatter;
    }

    static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T> formatter;

        static FormatterCache()
        {
            formatter = (IMessagePackFormatter<T>) BlackStringResolverGetFormatterHelper.GetFormatter(typeof(T));
        }
    }
}

internal static class BlackStringResolverGetFormatterHelper
{
    internal static object GetFormatter(Type t)
    {
        if (t == typeof(ScString))
            return BlackStringFormatter.Instance;
        if (t == typeof(ScString[])) return new ArrayFormatter<ScString>();

        return null;
    }
}