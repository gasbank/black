using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using ConditionalDebug;
using MessagePack;
using UnityEngine;
using UnityEngine.UI;

public class StageSaveManager : MonoBehaviour
{
    [SerializeField]
    PinchZoom pinchZoom;

    [SerializeField]
    RawImage targetImage;

    static void InitializeMessagePackConditional()
    {
    }

    public void Save(string stageName, HashSet<uint> coloredMinPoints, GridWorld gridWorld, float remainTime)
    {
        SaveWipStageData(stageName, coloredMinPoints, remainTime);
    }

    public static string GetStageSaveFileName(string stageName)
    {
        return stageName + ".save";
    }

    public static string GetWipPngFileName(string stageName)
    {
        return stageName + ".png";
    }

    void SaveWipStageData(string stageName, HashSet<uint> coloredMinPoints, float remainTime)
    {
        ConDebug.Log($"Saving save data for '{stageName}'...");
        InitializeMessagePackConditional();
        var bytes = MessagePackSerializer.Serialize(
            CreateWipStageSaveData(stageName, coloredMinPoints, remainTime, null),
            Data.DefaultOptions);
        FileUtil.SaveAtomically(GetStageSaveFileName(stageName), bytes);
    }

    public static void DeleteSaveFile(string stageName)
    {
        var saveDataPath = FileUtil.GetPath(GetStageSaveFileName(stageName));
        ConDebug.Log($"Deleting save file '{saveDataPath}'...");
        File.Delete(saveDataPath);

        var wipPngPath = FileUtil.GetPath(GetWipPngFileName(stageName));
        ConDebug.Log($"Deleting save file '{wipPngPath}'...");
        File.Delete(wipPngPath);
    }

    public static StageSaveData Load(string stageName)
    {
        try
        {
            ConDebug.Log($"Loading save data for '{stageName}'...");
            InitializeMessagePackConditional();
            var bytes = File.ReadAllBytes(FileUtil.GetPath(GetStageSaveFileName(stageName)));
            ConDebug.Log($"{bytes.Length} bytes loaded.");
            var stageSaveData = MessagePackSerializer.Deserialize<StageSaveData>(bytes, Data.DefaultOptions);
            return stageSaveData;
        }
        catch (FileNotFoundException)
        {
            ConDebug.Log("No save data exist.");
            return null;
        }
        catch (IsolatedStorageException)
        {
            ConDebug.Log("No save data exist.");
            return null;
        }
    }

    public StageSaveData CreateStageSaveData(string stageName)
    {
        return CreateWipStageSaveData(stageName, new HashSet<uint>(),
            StageButton.CurrentStageMetadata != null
                ? StageButton.CurrentStageMetadata.StageSequenceData.remainTime
                : 0, null);
    }

    public void RestoreCameraState(StageSaveData stageSaveData)
    {
        var targetImageTransform = targetImage.transform;
        var targetImageLocPos = targetImageTransform.localPosition;

        targetImageTransform.localPosition = new Vector3(stageSaveData.targetImageCenterX,
            stageSaveData.targetImageCenterY, targetImageLocPos.z);

        pinchZoom.ZoomValue = stageSaveData.zoomValue;
    }

    public static bool LoadWipPng(string stageName, Texture2D tex)
    {
        try
        {
            var bytesPng = File.ReadAllBytes(FileUtil.GetPath(GetWipPngFileName(stageName)));
            tex.LoadImage(bytesPng);
            return true;
        }
        catch (FileNotFoundException)
        {
        }

        return false;
    }

    StageSaveData CreateWipStageSaveData(string stageName, HashSet<uint> coloredMinPoints, float remainTime,
        byte[] png)
    {
        var targetImageLocPos = targetImage.transform.localPosition;
        var stageSaveData = new StageSaveData
        {
            stageName = stageName,
            coloredMinPoints = coloredMinPoints,
            zoomValue = pinchZoom.ZoomValue,
            targetImageCenterX = targetImageLocPos.x,
            targetImageCenterY = targetImageLocPos.y,
            remainTime = remainTime,
            png = png,
        };
        return stageSaveData;
    }
}