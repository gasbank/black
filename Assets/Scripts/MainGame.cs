using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class MainGame : MonoBehaviour {
    [SerializeField] Texture2D defaultTexture = null;
    [SerializeField] GridWorld gridWorld = null;
    [SerializeField] TextAsset islandData = null;
    [SerializeField] PaletteButtonGroup paletteButtonGroup = null;
    [SerializeField] IslandLabelSpawner islandLabelSpawner = null;
    StageData stageData;

    void Awake() {
        using (var stream = new MemoryStream(islandData.bytes)) {
            var formatter = new BinaryFormatter();
            stageData = (StageData)formatter.Deserialize(stream);
            stream.Close();
        }

        Debug.Log($"{stageData.islandDataByMinPoint.Count} islands loaded.");

        if (StageButton.currentStageTexture != null) {
            gridWorld.LoadTexture(StageButton.currentStageTexture, stageData);
        } else {
            gridWorld.LoadTexture(defaultTexture, stageData);
        }

        paletteButtonGroup.CreatePalette(stageData);

        islandLabelSpawner.CreateAllLabels(stageData);
    }
}
