using System;
using MessagePack;

[Serializable]
[MessagePackObject]
public class GameSaveData {
    [Key(0)] public ScInt gold;
}
