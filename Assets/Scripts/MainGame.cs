using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;
using ConditionalDebug;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGame : MonoBehaviour {
    [SerializeField] GridWorld gridWorld;
    [SerializeField] PaletteButtonGroup paletteButtonGroup;
    [SerializeField] IslandLabelSpawner islandLabelSpawner;
    [SerializeField] TargetImage targetImage;
    [SerializeField] Image targetImageOutline;
    [SerializeField] PinchZoom pinchZoom;
    [SerializeField] StageMetadata stageMetadata;

    StageData stageData;

    void Start() {
        Application.runInBackground = false;

        if (gridWorld == null) {
            return;
        }

        // Stage Selection 신에서 넘어왔다면 이 조건문이 만족할 것이다.
        if (StageButton.CurrentStageMetadata != null) {
            stageMetadata = StageButton.CurrentStageMetadata;
            ConDebug.Log($"Stage metadata specified by StageButton: {stageMetadata.name}");
        }

        using (var stream = new MemoryStream(stageMetadata.RawStageData.bytes)) {
            var formatter = new BinaryFormatter();
            stageData = (StageData)formatter.Deserialize(stream);
            stream.Close();
        }
        stageData.islandCountByColor = stageData.islandDataByMinPoint.GroupBy(g => g.Value.rgba).ToDictionary(g => g.Key, g => g.Count());

        Debug.Log($"{stageData.islandDataByMinPoint.Count} islands loaded.");
        var maxIslandPixelArea = stageData.islandDataByMinPoint.Max(e => e.Value.pixelArea);
        Debug.Log($"Max island pixel area: {maxIslandPixelArea}");

        var instantiateMaterials = true;
        if (instantiateMaterials) {
            var skipBlackMaterial = Instantiate(stageMetadata.SkipBlackMaterial);
            var colorTexture = Instantiate((Texture2D)skipBlackMaterial.GetTexture("ColorTexture"));
            skipBlackMaterial.SetTexture("ColorTexture", colorTexture);

            gridWorld.LoadTexture(colorTexture, stageData, maxIslandPixelArea);
            gridWorld.StageName = stageMetadata.name;
            targetImage.SetTargetImageMaterial(skipBlackMaterial);
        } else {
            var skipBlackMaterial = stageMetadata.SkipBlackMaterial;
            var colorTexture = (Texture2D)skipBlackMaterial.GetTexture("ColorTexture");
            
            gridWorld.LoadTexture(colorTexture, stageData, maxIslandPixelArea);
            gridWorld.StageName = stageMetadata.name;
            targetImage.SetTargetImageMaterial(skipBlackMaterial);
        }

        targetImageOutline.material = stageMetadata.SdfMaterial;
        // SDF 머티리얼 없으면 아예 이 이미지는 안보이게 하자.
        targetImageOutline.enabled = stageMetadata.SdfMaterial != null;

        paletteButtonGroup.CreatePalette(stageData);

        islandLabelSpawner.CreateAllLabels(stageData);

        //gridWorld.FloodFillVec2IntAndApply(1208, 716, true);

        var counts = gridWorld.CountWhiteAndBlackInBitmap();
        ConDebug.Log($"Tex size: {gridWorld.texSize}");
        ConDebug.Log($"Black count: {counts[0]}");
        ConDebug.Log($"White count: {counts[1]}");
        ConDebug.Log($"Other count: {counts[2]}");

        //gridWorld.FloodFillVec2IntAndApplyWithSolution(BlackConvert.GetInvertedY(new Vector2Int(922, 1202), gridWorld.texSize));

        gridWorld.ResumeGame();
    }

    public void ResetCamera() {
        targetImage.transform.localPosition = new Vector3(0, 0, targetImage.transform.localPosition.z);
        pinchZoom.ResetZoom();
    }

    int resetCount;

    public void ResetStage() {
        resetCount++;
        if (resetCount > 5) {
            gridWorld.DeleteSaveFileAndReloadScene();
        }
    }

    public void LoadStageSelectionScene() {
        if (gridWorld != null) {
            gridWorld.WriteStageSaveData();
        }
        SceneManager.LoadScene("Stage Selection");
    }

    public void LoadMuseumScene() {
        if (gridWorld != null) {
            gridWorld.WriteStageSaveData();
        }
        SceneManager.LoadScene("Museum");
    }
}
