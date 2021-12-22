using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ConditionalDebug;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainGame : MonoBehaviour
{
    static bool Verbose => false;
    
    static readonly int ColorTexture = Shader.PropertyToID("ColorTexture");

    [SerializeField]
    GridWorld gridWorld;

    [SerializeField]
    IslandLabelSpawner islandLabelSpawner;

    [SerializeField]
    NameplateGroup nameplateGroup;

    [SerializeField]
    PaletteButtonGroup paletteButtonGroup;

    [SerializeField]
    CanvasGroup achieveGroup;

    [SerializeField]
    PinchZoom pinchZoom;

    StageData stageData;

    [SerializeField]
    StageMetadata stageMetadata;

    [SerializeField]
    TargetImage targetImage;

    [SerializeField]
    Image targetImageOutline;

    public bool CanInteractPanAndZoom => islandLabelSpawner.IsLabelByMinPointEmpty == false;

    void Start()
    {
        Application.runInBackground = false;

        if (gridWorld == null) return;

        // Stage Selection 신에서 넘어왔다면 이 조건문이 만족할 것이다.
        if (StageButton.CurrentStageMetadata != null)
        {
            stageMetadata = StageButton.CurrentStageMetadata;

            if (Verbose)
            {
                ConDebug.Log($"Stage metadata specified by StageButton: {stageMetadata.name}");
            }
        }

        using (var stream = new MemoryStream(stageMetadata.RawStageData.bytes))
        {
            var formatter = new BinaryFormatter();
            stageData = (StageData) formatter.Deserialize(stream);
            stream.Close();
        }

        stageData.islandCountByColor = stageData.islandDataByMinPoint.GroupBy(g => g.Value.rgba)
            .ToDictionary(g => g.Key, g => g.Count());

        if (Verbose)
        {
            ConDebug.Log($"{stageData.islandDataByMinPoint.Count} islands loaded.");
        }

        var maxIslandPixelArea = stageData.islandDataByMinPoint.Max(e => e.Value.pixelArea);
        if (Verbose)
        {
            foreach (var mp in stageData.islandDataByMinPoint)
            {
                ConDebug.Log($"Island: Key={mp.Key} PixelArea={mp.Value.pixelArea}");
            }

            ConDebug.Log($"Max island pixel area: {maxIslandPixelArea}");
        }

        var skipBlackMaterial = Instantiate(stageMetadata.SkipBlackMaterial);
        var colorTexture = Instantiate((Texture2D) skipBlackMaterial.GetTexture(ColorTexture));
        skipBlackMaterial.SetTexture(ColorTexture, colorTexture);

        gridWorld.LoadTexture(colorTexture, stageData, maxIslandPixelArea);
        gridWorld.StageName = stageMetadata.name;
        nameplateGroup.Text = stageMetadata.FriendlyStageName;

        targetImage.SetTargetImageMaterial(skipBlackMaterial);

        targetImageOutline.material = stageMetadata.SdfMaterial;
        // SDF 머티리얼 없으면 아예 이 이미지는 안보이게 하자.
        targetImageOutline.enabled = stageMetadata.SdfMaterial != null;

        paletteButtonGroup.CreatePalette(stageData);

        islandLabelSpawner.CreateAllLabels(stageData);

        var counts = gridWorld.CountWhiteAndBlackInBitmap();
        if (Verbose)
        {
            ConDebug.Log($"Tex size: {gridWorld.TexSize}");
            ConDebug.Log($"Black count: {counts[0]}");
            ConDebug.Log($"White count: {counts[1]}");
            ConDebug.Log($"Other count: {counts[2]}");
            ConDebug.Log($"CanInteractPanAndZoom = {CanInteractPanAndZoom}");
        }

        gridWorld.ResumeGame();
    }

    public void ResetCamera()
    {
        var targetImageTransform = targetImage.transform;
        targetImageTransform.localPosition = new Vector3(0, 0, targetImageTransform.localPosition.z);
        pinchZoom.ResetZoom();
    }

    public void ResetStage()
    {
        gridWorld.DeleteSaveFileAndReloadScene();
    }

    public void LoadStageSelectionScene()
    {
        if (gridWorld != null) gridWorld.WriteStageSaveData();

        SceneManager.LoadScene("Stage Selection");
    }

    public void OnFinishConfirmButton()
    {
        if (gridWorld != null) gridWorld.WriteStageSaveData();

        if (BlackContext.instance.PendingGold == 1)
        {
            ConfirmPopup.instance.Open(@"\클리어를 축하합니다. 1골드를 받았습니다.", () => SceneManager.LoadScene("Stage Selection"));
        }
        else
        {
            SceneManager.LoadScene("Stage Selection");
        }
    }

    public void LoadMuseumScene()
    {
        if (gridWorld != null) gridWorld.WriteStageSaveData();

        SceneManager.LoadScene("Museum");
    }

    public void AchievePopup(bool show)
    {
        if (show) achieveGroup.Show();
        else achieveGroup.Hide();
    }
}