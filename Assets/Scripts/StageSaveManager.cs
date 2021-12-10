﻿using System;
using System.Collections.Generic;
using System.IO;
using ConditionalDebug;
using MessagePack;
using UnityEngine;
using UnityEngine.UI;

public class StageSaveManager : MonoBehaviour {
    [SerializeField] PinchZoom pinchZoom;
    [SerializeField] Image targetImage;

    static readonly string GameName = "game";

    static void InitializeMessagePackConditional() {
    }

    public void Save(string stageName, HashSet<uint> coloredMinPoints, GridWorld gridWorld) {
        SaveStageData(stageName, coloredMinPoints);
        SaveGameData(gridWorld);
        SaveWipPngData(stageName, gridWorld);
    }

    string GetStageSaveFileName(string stageName) => stageName + ".save";
    static string GetWipPngFileName(string stageName) => stageName + ".png";

    void SaveWipPngData(string stageName, GridWorld gridWorld) {
        FileUtil.SaveAtomically(GetWipPngFileName(stageName), gridWorld.Tex.EncodeToPNG());
        Debug.Log($"WIP PNG '{GetWipPngFileName(stageName)}' written.");
    }

    void SaveStageData(string stageName, HashSet<uint> coloredMinPoints) {
        ConDebug.Log($"Saving save data for '{stageName}'...");
        InitializeMessagePackConditional();
        FileUtil.SaveAtomically(GetStageSaveFileName(stageName), MessagePackSerializer.Serialize(CreateStageSaveData(stageName, coloredMinPoints), Data.DefaultOptions));
    }

    void SaveGameData(GridWorld gridWorld) {
        ConDebug.Log($"Saving game data...");
        InitializeMessagePackConditional();
        FileUtil.SaveAtomically(GameName, MessagePackSerializer.Serialize(CreateGameSaveData(gridWorld), Data.DefaultOptions));
    }

    public void DeleteSaveFile(string stageName) {
        var saveDataPath = FileUtil.GetPath(GetStageSaveFileName(stageName));
        Debug.Log($"Deleting save file '{saveDataPath}'...");
        File.Delete(saveDataPath);

        var wipPngPath = FileUtil.GetPath(GetWipPngFileName(stageName));
        Debug.Log($"Deleting save file '{wipPngPath}'...");
        File.Delete(wipPngPath);
    }

    public StageSaveData Load(string stageName) {
        try {
            ConDebug.Log($"Loading save data for '{stageName}'...");
            InitializeMessagePackConditional();
            var bytes = File.ReadAllBytes(FileUtil.GetPath(GetStageSaveFileName(stageName)));
            ConDebug.Log($"{bytes.Length} bytes loaded.");
            var stageSaveData = MessagePackSerializer.Deserialize<StageSaveData>(bytes, Data.DefaultOptions);
            targetImage.transform.localPosition = new Vector3(stageSaveData.targetImageCenterX, stageSaveData.targetImageCenterY, targetImage.transform.localPosition.z);
            pinchZoom.ZoomValue = stageSaveData.zoomValue;
            return stageSaveData;
        } catch (FileNotFoundException) {
            ConDebug.Log($"No save data exist.");
            return CreateStageSaveData(stageName, new HashSet<uint>());
        } catch (System.IO.IsolatedStorage.IsolatedStorageException) {
            ConDebug.Log($"No save data exist.");
            return CreateStageSaveData(stageName, new HashSet<uint>());
        }
    }

    public static bool LoadWipPng(string stageName, Texture2D tex) {
        try {
            var bytesPng = File.ReadAllBytes(FileUtil.GetPath(GetWipPngFileName(stageName)));
            tex.LoadImage(bytesPng);
            return true;
        } catch (FileNotFoundException) {
        }
        return false;
    }

    StageSaveData CreateStageSaveData(string stageName, HashSet<uint> coloredMinPoints) {
        var stageSaveData = new StageSaveData {
            stageName = stageName,
            coloredMinPoints = coloredMinPoints,
            zoomValue = pinchZoom.ZoomValue,
            targetImageCenterX = targetImage.transform.localPosition.x,
            targetImageCenterY = targetImage.transform.localPosition.y,
        };
        return stageSaveData;
    }

    GameSaveData CreateGameSaveData(GridWorld gridWorld) {
        var gameSaveData = new GameSaveData {
            gold = gridWorld.Gold,
        };
        return gameSaveData;
    }
}
