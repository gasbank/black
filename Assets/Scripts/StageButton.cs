using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButton : MonoBehaviour {
    public static StageMetadata CurrentStageMetadata { get; private set; }

    //[SerializeField] Image stageImage = null;
    [SerializeField] StageMetadata stageMetadata = null;
    
    public void GoToMain() {
        CurrentStageMetadata = stageMetadata;
        SceneManager.LoadScene("Main");
    }
}
