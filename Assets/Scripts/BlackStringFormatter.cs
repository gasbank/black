using System;
using MessagePack;
using MessagePack.Formatters;
using UnityEngine;

public sealed class BlackStringFormatter : IMessagePackFormatter<ScString>
{
    public static readonly BlackStringFormatter Instance = new BlackStringFormatter();

    static int errorStringCounter;

    public void Serialize(ref MessagePackWriter writer, ScString value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        int strNo;

        if (BlackStringTable.StringNumberDict.TryGetValue(value, out var existingStrNo))
        {
            strNo = existingStrNo;
        }
        else
        {
            strNo = BlackStringTable.StringNumberDict.Count + 1;
            BlackStringTable.StringNumberDict[value] = strNo;
        }

        if (strNo > ushort.MaxValue) throw new Exception("String number exceeds 65,535");

        writer.Write(strNo);
    }

    public ScString Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil)
        {
            reader.Skip();
            return null;
        }

        var strNo = reader.ReadInt32();
        if (BlackStringTable.ScStringMap != null && strNo >= 1 && strNo <= BlackStringTable.ScStringMap.Length)
        {
            return BlackStringTable.ScStringMap[strNo - 1];
        }

        Debug.LogError(
            $"String number {strNo} not found on string map! BlackStringTable.ScStringMap={BlackStringTable.ScStringMap}, BlackStringTable.ScStringMap.Length={BlackStringTable.ScStringMap?.Length ?? -1}");
        // Dictionary 키로 쓰이는 문자열도 있어서 숫자 하나씩 증가시켜준다.
        errorStringCounter++;
        if (errorStringCounter >= 100) throw new UnityException("Too many string number not found.");
        return new ScString($"STRING NUMBER NOT FOUND [{errorStringCounter}]");
    }
}