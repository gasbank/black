using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

[Serializable]
[MessagePackObject]
public class StageSaveData {
    [Key(0)] public string stageName;
    [Key(1)] public HashSet<uint> coloredMinPoints = new HashSet<uint>();
    [Key(2)] public float zoomValue;
    [Key(3)] public float targetImageCenterX;
    [Key(4)] public float targetImageCenterY;
}
