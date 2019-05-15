using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButton : MonoBehaviour {
    [SerializeField] Image stageImage = null;
    public static Texture2D currentStageTexture = null;

    public void GoToMain() {
        currentStageTexture = stageImage.mainTexture as Texture2D;
        SceneManager.LoadScene("Main");
    }
}
