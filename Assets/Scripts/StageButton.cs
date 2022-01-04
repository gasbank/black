using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    static readonly int ColorTexture = Shader.PropertyToID("ColorTexture");

    [SerializeField]
    Image image;

    [SerializeField]
    StageMetadata stageMetadata;

    [SerializeField]
    Text stageNumber;

    [SerializeField]
    StageStar stageStar;

    [SerializeField]
    bool updateOnStart;

    public static StageMetadata CurrentStageMetadata { get; private set; }

    public void GoToMain()
    {
        CurrentStageMetadata = stageMetadata;
        SceneManager.LoadScene("Main");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (stageNumber != null) stageNumber.text = (transform.GetSiblingIndex() + 1).ToString();
    }
#endif

    void Start()
    {
        if (updateOnStart)
        {
            UpdateButtonImage();
        }
    }

    bool UpdateButtonImage()
    {
        var resumed = false;
        
        if (stageMetadata == null)
        {
            return resumed;
        }

        var wipTex2D = new Texture2D(512, 512, TextureFormat.RGB24, true, true);
        if (StageSaveManager.LoadWipPng(stageMetadata.name, wipTex2D))
        {
            image.material = Instantiate(image.material);
            image.material.mainTexture = wipTex2D;
            image.sprite = null;
            resumed = true;
        }
        else
        {
            var blankTex2D = stageMetadata.SkipBlackMaterial.GetTexture(ColorTexture) as Texture2D;
            if (blankTex2D != null)
            {
                image.sprite = Sprite.Create(blankTex2D, new Rect(0, 0, blankTex2D.width, blankTex2D.height),
                    Vector2.one / 2);
            }
            else
            {
                Debug.LogError("Skip Black Material's Color Texture is null");
            }
        }

        if (stageStar != null)
        {
            stageStar.StarCount = stageMetadata.StageSequenceData.starCount;
        }

        return resumed;
    }

    public bool SetStageMetadata(StageMetadata inStageMetadata)
    {
        stageMetadata = inStageMetadata;
        return UpdateButtonImage();
    }

    public void SetStageMetadataToCurrent()
    {
        if (stageMetadata == null)
        {
            Debug.LogError("Stage metadata is null");
            return;
        }
        
        CurrentStageMetadata = stageMetadata;
    }
}