using System;
using MessagePack;

[Serializable]
[MessagePackObject]
public class StageSequenceData
{
    [Key(0)]
    public int seq;

    [Key(1)]
    public string stageName;
}