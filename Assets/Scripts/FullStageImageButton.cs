using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class FullStageImageButton : MonoBehaviour
{
    [SerializeField]
    IslandShader3DController islandShader3DController;

    [SerializeField]
    StageDetailPopup stageDetailPopupForReplay;

    [SerializeField]
    RawImage rawImage;

    [SerializeField]
    Button button;

    [SerializeField]
    Color loadingColor = new Color(0.9019608f, 0.8901961f, 0.8352941f, 1.0f);

    [SerializeField]
    int stageIndex;

    Material stageMaterialTemplate;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
        CacheReferences();
        CacheStageMaterialTemplate();
    }
#endif

    void Awake()
    {
        CacheReferences();
        CacheStageMaterialTemplate();
    }

    void CacheReferences()
    {
        if (islandShader3DController == null)
        {
            islandShader3DController = GetComponent<IslandShader3DController>();
        }

        if (rawImage == null)
        {
            rawImage = GetComponent<RawImage>();
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    void CacheStageMaterialTemplate()
    {
        if (stageMaterialTemplate == null && rawImage != null)
        {
            stageMaterialTemplate = rawImage.material;
        }
    }

    public async void OnClick()
    {
        Sound.Instance.PlayButtonClick();
        
        await stageDetailPopupForReplay.OpenPopupAfterLoadingAsync(stageIndex);
    }

    public void InitializeShell(int inStageIndex, StageDetailPopup inStageDetailPopupForReplay)
    {
        CacheReferences();
        CacheStageMaterialTemplate();

        stageIndex = inStageIndex;
        stageDetailPopupForReplay = inStageDetailPopupForReplay;

        if (button != null)
        {
            button.interactable = true;
        }

        SetLoadingState(true);
    }

    public void SetLoadingState(bool isLoading)
    {
        CacheReferences();
        CacheStageMaterialTemplate();

        if (rawImage == null)
        {
            return;
        }

        if (isLoading)
        {
            rawImage.material = null;
            rawImage.texture = Texture2D.whiteTexture;
            rawImage.color = loadingColor;
            return;
        }

        rawImage.material = stageMaterialTemplate;
        rawImage.texture = null;
        rawImage.color = Color.white;
    }

    public void BindStageMetadata(StageMetadata stageMetadata)
    {
        SetLoadingState(false);
        islandShader3DController.Initialize(stageMetadata);
    }

    public void Initialize(StageMetadata stageMetadata, StageDetailPopup inStageDetailPopupForReplay)
    {
        InitializeShell(stageMetadata.StageIndex, inStageDetailPopupForReplay);
        BindStageMetadata(stageMetadata);
    }
}
