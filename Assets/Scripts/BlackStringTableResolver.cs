﻿using System;
using MessagePack;
using MessagePack.Formatters;

public sealed class BlackStringTableResolver : IFormatterResolver
{
    public static readonly IFormatterResolver Instance = new BlackStringTableResolver();

    BlackStringTableResolver()
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
            formatter = (IMessagePackFormatter<T>) BlackStringTableResolverGetFormatterHelper.GetFormatter(typeof(T));
        }
    }
}

internal static class BlackStringTableResolverGetFormatterHelper
{
    internal static object GetFormatter(Type t)
    {
        if (t == typeof(ScString))
            return BlackStringTableFormatter.Instance;
        if (t == typeof(ScString[])) return new ArrayFormatter<ScString>();

        return null;
    }
}