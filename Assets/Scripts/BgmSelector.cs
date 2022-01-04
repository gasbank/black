using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class BgmSelector : MonoBehaviour
{
    [SerializeField]
    AudioSource bgmSource;
    
    [SerializeField]
    AudioClip lobbyBgm;

    [SerializeField]
    AudioClip[] mainBgm;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Stage Selection")
        {
            bgmSource.clip = lobbyBgm;
        }
        else
        {
            var stageIndex = GetCurrentStageIndex();
            
            if (mainBgm.Length > 0 && stageIndex >= 0)
            {
                bgmSource.clip = mainBgm[stageIndex % mainBgm.Length];
            }
        }

        if (bgmSource.isActiveAndEnabled)
        {
            bgmSource.Play();
        }
    }

    static int GetCurrentStageIndex()
    {
        if (StageButton.CurrentStageMetadata == null
            || Data.dataSet == null
            || Data.dataSet.StageSequenceData == null)
        {
            return -1;
        }
        
        var stageName = StageButton.CurrentStageMetadata.name;
        for (var i = 0; i < Data.dataSet.StageSequenceData.Count; i++)
        {
            if (Data.dataSet.StageSequenceData[i] != null
                && Data.dataSet.StageSequenceData[i].stageName == stageName)
            {
                return i;
            }
        }

        return -1;
    }
}