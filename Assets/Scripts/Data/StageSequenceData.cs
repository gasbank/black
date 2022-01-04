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
    
    [Key(2)]
    public string artist;
    
    [Key(3)]
    public string title;
    
    [Key(4)]
    public string desc;
    
    [Key(5)]
    public int starCount;

    [Key(6)]
    public float remainTime;
    
    [Key(7)]
    public bool skipLock;
}