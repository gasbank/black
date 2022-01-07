using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using ConditionalDebug;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainGame : MonoBehaviour
{
    static bool Verbose => false;

    static readonly int ColorTexture = Shader.PropertyToID("ColorTexture");

    public static MainGame instance;

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

    [SerializeField]
    GameObject timeGroup;

    [SerializeField]
    Text timeText;

    [SerializeField]
    float remainTime;

    public bool CanInteractPanAndZoom => islandLabelSpawner.IsLabelByMinPointEmpty == false;

    async void Start()
    {
        Application.runInBackground = false;

        if (gridWorld == null) return;

        // Lobby 신에서 넘어왔다면 이 조건문이 만족할 것이다.
        if (StageButton.CurrentStageMetadata != null)
        {
            stageMetadata = StageButton.CurrentStageMetadata;

            if (Verbose)
            {
                ConDebug.Log($"Stage metadata specified by StageButton: {stageMetadata.name}");
            }
        }

        if (stageMetadata == null)
        {
            while (Data.dataSet == null || Data.dataSet.StageMetadataLocList == null)
            {
                await Task.Yield();
            }

            stageMetadata = await Addressables.LoadAssetAsync<StageMetadata>(Data.dataSet.StageMetadataLocList[0]).Task;
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
        nameplateGroup.ArtistText = stageMetadata.StageSequenceData.artist;
        nameplateGroup.TitleText = stageMetadata.StageSequenceData.title;
        nameplateGroup.DescText = stageMetadata.StageSequenceData.desc;

        targetImage.SetTargetImageMaterial(skipBlackMaterial);

        targetImageOutline.material = stageMetadata.SdfMaterial;
        // SDF 머티리얼 없으면 아예 이 이미지는 안보이게 하자.
        targetImageOutline.enabled = stageMetadata.SdfMaterial != null;

        paletteButtonGroup.CreatePalette(stageData);

        islandLabelSpawner.CreateAllLabels(stageData);

        var counts = gridWorld.CountWhiteAndBlackInBitmap();

        remainTime = stageMetadata.StageSequenceData.remainTime;

        if (stageMetadata.StageSequenceData.remainTime > 0)
        {
            ActivateTime();
        }
        else
        {
            DeactivateTime();
        }

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

    public void GoToLobby()
    {
        if (gridWorld != null) gridWorld.WriteStageSaveData();

        SceneManager.LoadScene("Lobby");
    }

    public void OnFinishConfirmButton()
    {
        Sound.instance.PlayButtonClick();

        if (gridWorld != null) gridWorld.WriteStageSaveData();

        if (gridWorld.RewardGoldAmount > 0)
        {
            ConfirmPopup.instance.Open(@"\클리어를 축하합니다. {0}골드를 받았습니다.".Localized(gridWorld.RewardGoldAmount),
                () => SceneManager.LoadScene("Lobby"));

            Sound.instance.PlaySoftTada();
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    public void ToggleComboAdminMode()
    {
        BlackContext.instance.ComboAdminMode = !BlackContext.instance.ComboAdminMode;
    }

    public void LoadMuseumScene()
    {
        if (gridWorld != null) gridWorld.WriteStageSaveData();

        // ReSharper disable once Unity.LoadSceneUnknownSceneName
        SceneManager.LoadScene("Museum");
    }

    // ReSharper disable once UnusedMember.Global
    public void AchievePopup(bool show)
    {
        if (show) achieveGroup.Show();
        else achieveGroup.Hide();
    }

    void Update()
    {
        if (timeGroup.gameObject.activeInHierarchy)
        {
            remainTime = Mathf.Max(0, remainTime - Time.deltaTime);
            GetMinutesSeconds(TimeSpan.FromSeconds(remainTime), out var minutes, out var seconds);
            timeText.text = @"\남은 시간".Localized() + "\n" + $"{minutes:00}:{seconds:00}";

            if (remainTime <= 0)
            {
                DeactivateTime();
                gridWorld.DeleteSaveFile();
                ConfirmPopup.instance.Open("제한 시간이 지났습니다. 처음부터 다시 시작해야합니다.",
                    () => SceneManager.LoadScene("Lobby"));
            }
        }
    }

    static void GetMinutesSeconds(TimeSpan totalElapsedTimeSpan, out int minutes, out int seconds)
    {
        int.TryParse(totalElapsedTimeSpan.ToString("%m"), out minutes);
        int.TryParse(totalElapsedTimeSpan.ToString("%s"), out seconds);
    }

    public void DeactivateTime()
    {
        timeGroup.gameObject.SetActive(false);
    }

    void ActivateTime()
    {
        timeGroup.gameObject.SetActive(true);
    }

    public void SetRemainTime(float f)
    {
        remainTime = f;
    }

    public float GetRemainTime()
    {
        return remainTime;
    }

    public void OpenResetStageConfirmPopup()
    {
        ConfirmPopup.instance.OpenYesNoPopup(@"\이 스테이지를 처음부터 새로 시작하겠습니까?".Localized(),
            ResetStage,
            ConfirmPopup.instance.Close);
    }
}