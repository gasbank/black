using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MainGame : MonoBehaviour {
    [SerializeField] Texture2D defaultTexture = null;
    [SerializeField] GridWorld gridWorld = null;
    [SerializeField] TextAsset islandData = null;
    [SerializeField] PaletteButtonGroup paletteButtonGroup = null;
    [SerializeField] IslandLabelSpawner islandLabelSpawner = null;
    [SerializeField] TargetImage targetImage = null;
    [SerializeField] PinchZoom pinchZoom = null;

    StageData stageData;

    void Start() {
        Application.runInBackground = false;

        using (var stream = new MemoryStream(islandData.bytes)) {
            var formatter = new BinaryFormatter();
            stageData = (StageData)formatter.Deserialize(stream);
            stream.Close();
        }

        Debug.Log($"{stageData.islandDataByMinPoint.Count} islands loaded.");
        var maxIslandPixelArea = stageData.islandDataByMinPoint.Max(e => e.Value.pixelArea);
        Debug.Log($"Max island pixel area: {maxIslandPixelArea}");

        if (StageButton.currentStageTexture != null) {
            gridWorld.LoadTexture(StageButton.currentStageTexture, stageData, maxIslandPixelArea);
        } else {
            var copiedTex = gridWorld.LoadTexture(defaultTexture, stageData, maxIslandPixelArea);
            targetImage.SetTargetImageMaterialTexture(copiedTex);
        }

        paletteButtonGroup.CreatePalette(stageData);

        islandLabelSpawner.CreateAllLabels(stageData);

        //gridWorld.FloodFillVec2IntAndApply(1208, 716, true);
        
        var counts = gridWorld.CountWhiteAndBlackInBitmap();
        SushiDebug.Log($"Tex size: {gridWorld.texSize}");
        SushiDebug.Log($"Black count: {counts[0]}");
        SushiDebug.Log($"White count: {counts[1]}");
        SushiDebug.Log($"Other count: {counts[2]}");

        //gridWorld.FloodFillVec2IntAndApplyWithSolution(BlackConvert.GetInvertedY(new Vector2Int(922, 1202), gridWorld.texSize));

         //gridWorld.ResumeGame();
    }

    public void ResetCamera() {
        targetImage.transform.localPosition = new Vector3(0, 0, targetImage.transform.localPosition.z);
        pinchZoom.ResetZoom(); 
    }
}
