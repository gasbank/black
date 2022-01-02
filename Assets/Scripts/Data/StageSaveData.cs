using System;
using System.Collections.Generic;
using MessagePack;

[Serializable]
[MessagePackObject]
public class StageSaveData
{
    [Key(1)]
    public HashSet<uint> coloredMinPoints = new HashSet<uint>();

    [Key(0)]
    public string stageName;

    [Key(3)]
    public float targetImageCenterX;

    [Key(4)]
    public float targetImageCenterY;

    [Key(2)]
    public float zoomValue;

    [Key(5)]
    public float remainTime;

    [Key(6)]
    public byte[] png;
}