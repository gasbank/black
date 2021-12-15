using System;
using MessagePack;

[Serializable]
[MessagePackObject]
public class StageSequenceData
{
    [Key(0)]
    public int stageId;

    [Key(1)]
    public string stageName;
}