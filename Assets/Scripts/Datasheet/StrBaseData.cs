using MessagePack;

[System.Serializable]
[MessagePackObject]
public class StrBaseData {
    [Key(0)] public ScString id;
    [Key(1)] public ScString[] str;
}
