using System.Collections.Generic;
using MessagePack;

[System.Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class DataSetHash {
    public Dictionary<string, string> Hash;
}
