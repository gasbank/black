using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButton : MonoBehaviour {
    public static StageMetadata CurrentStageMetadata { get; private set; }

    //[SerializeField] Image stageImage = null;
    [SerializeField] StageMetadata stageMetadata = null;
    [SerializeField] Text stageNumber = null;
    [SerializeField] Image image = null;

    public void GoToMain() {
        CurrentStageMetadata = stageMetadata;
        SceneManager.LoadScene("Main");
    }

    void OnValidate() {
        if (stageNumber != null) {
            stageNumber.text = (transform.GetSiblingIndex() + 1).ToString();
        }
    }

    void Start() {
        var tex2D = new Texture2D(512, 512, TextureFormat.RGB24, true, true);
        if (StageSaveManager.LoadWipPng(stageMetadata.name, tex2D)) {
            image.material = Instantiate(image.material);
            image.material.mainTexture = tex2D;
            image.sprite = null;
        }
    }
}
