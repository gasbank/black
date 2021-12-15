using System;
using System.Collections.Generic;
using MessagePack;

[Serializable]
[MessagePackObject(true)]
public class DataSetHash
{
    public Dictionary<string, string> Hash;
}