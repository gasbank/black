using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class StageSaveManager : MonoBehaviour {
    [SerializeField] PinchZoom pinchZoom = null;
    [SerializeField] Image targetImage = null;

    static readonly string GameName = "game";

    private static void InitializeMessagePackConditional() {
        if (MessagePack.MessagePackSerializer.IsInitialized == false) {
            // 두 번 호출하면 오류난다.
            MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
                MessagePack.Resolvers.GeneratedResolver.Instance,
                //MessagePack.Resolvers.DynamicGenericResolver.Instance,
                //MessagePack.Unity.UnityResolver.Instance,
                MessagePack.Resolvers.BuiltinResolver.Instance
            );
        }
    }

    public void Save(string stageName, HashSet<uint> coloredMinPoints, GridWorld gridWorld) {
        SaveStageData(stageName, coloredMinPoints);
        SaveGameData(gridWorld);
    }

    private void SaveStageData(string stageName, HashSet<uint> coloredMinPoints) {
        SushiDebug.Log($"Saving save data for '{stageName}'...");
        InitializeMessagePackConditional();
        FileUtil.SaveAtomically(stageName, MessagePack.LZ4MessagePackSerializer.Serialize(CreateStageSaveData(stageName, coloredMinPoints)));
    }

    private void SaveGameData(GridWorld gridWorld) {
        SushiDebug.Log($"Saving game data...");
        InitializeMessagePackConditional();
        FileUtil.SaveAtomically(GameName, MessagePack.LZ4MessagePackSerializer.Serialize(CreateGameSaveData(gridWorld)));
    }

    public void DeleteSaveFile(string stageName) {
        var saveDataPath = FileUtil.GetPath(stageName);
        Debug.Log($"Deleting save file '{saveDataPath}'...");
        File.Delete(saveDataPath);
    }

    public StageSaveData Load(string stageName) {
        try {
            SushiDebug.Log($"Loading save data for '{stageName}'...");
            InitializeMessagePackConditional();
            var bytes = File.ReadAllBytes(FileUtil.GetPath(stageName));
            SushiDebug.Log($"{bytes.Length} bytes loaded.");
            var stageSaveData = MessagePack.LZ4MessagePackSerializer.Deserialize<StageSaveData>(bytes);
            targetImage.transform.localPosition = new Vector3(stageSaveData.targetImageCenterX, stageSaveData.targetImageCenterY, targetImage.transform.localPosition.z);
            pinchZoom.ZoomValue = stageSaveData.zoomValue;
            return stageSaveData;
        } catch (FileNotFoundException) {
            SushiDebug.Log($"No save data exist.");
            return CreateStageSaveData(stageName, new HashSet<uint>());
        } catch (System.IO.IsolatedStorage.IsolatedStorageException) {
            SushiDebug.Log($"No save data exist.");
            return CreateStageSaveData(stageName, new HashSet<uint>());
        }
    }

    private StageSaveData CreateStageSaveData(string stageName, HashSet<uint> coloredMinPoints) {
        var stageSaveData = new StageSaveData {
            stageName = stageName,
            coloredMinPoints = coloredMinPoints,
            zoomValue = pinchZoom.ZoomValue,
            targetImageCenterX = targetImage.transform.localPosition.x,
            targetImageCenterY = targetImage.transform.localPosition.y,
        };
        return stageSaveData;
    }

    private GameSaveData CreateGameSaveData(GridWorld gridWorld) {
        var gameSaveData = new GameSaveData {
            gold = gridWorld.Gold,
        };
        return gameSaveData;
    }
}
