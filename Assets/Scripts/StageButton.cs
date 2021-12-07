using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    public static StageMetadata CurrentStageMetadata { get; private set; }

    //[SerializeField] Image stageImage = null;
    [SerializeField]
    StageMetadata stageMetadata;

    [SerializeField]
    Text stageNumber;

    [SerializeField]
    Image image;

    static readonly int ColorTexture = Shader.PropertyToID("ColorTexture");

    public void GoToMain()
    {
        CurrentStageMetadata = stageMetadata;
        SceneManager.LoadScene("Main");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (stageNumber != null)
        {
            stageNumber.text = (transform.GetSiblingIndex() + 1).ToString();
        }
    }
#endif

    void UpdateButtonImage()
    {
        var wipTex2D = new Texture2D(512, 512, TextureFormat.RGB24, true, true);
        if (StageSaveManager.LoadWipPng(stageMetadata.name, wipTex2D))
        {
            image.material = Instantiate(image.material);
            image.material.mainTexture = wipTex2D;
            image.sprite = null;
        }
        else
        {
            var blankTex2D = stageMetadata.SkipBlackMaterial.GetTexture(ColorTexture) as Texture2D;
            image.sprite = Sprite.Create(blankTex2D, new Rect(0, 0, blankTex2D.width, blankTex2D.height), Vector2.one / 2);
        }
    }

    public void SetStageMetadata(StageMetadata inStageMetadata)
    {
        stageMetadata = inStageMetadata;
        UpdateButtonImage();
    }
}