using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    [SerializeField]
    StageMetadata stageMetadata;

    [SerializeField]
    Text stageNumber;

    [SerializeField]
    StageStar stageStar;

    [SerializeField]
    bool updateOnStart;

    [SerializeField]
    StageLocker stageLocker;

    public static StageMetadata CurrentStageMetadata { get; private set; }
    public static bool CurrentStageMetadataReplay { get; private set; }

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
            return false;
        }

        var stageSaveData = StageSaveManager.Load(stageMetadata.name);

        if (stageSaveData != null)
        {
            resumed = true;
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

    public StageMetadata GetStageMetadata() => stageMetadata;

    public void SetStageMetadataToCurrent(bool replay)
    {
        if (stageMetadata == null)
        {
            Debug.LogError("Stage metadata is null");
            return;
        }
        
        CurrentStageMetadata = stageMetadata;
        CurrentStageMetadataReplay = replay;
    }
}