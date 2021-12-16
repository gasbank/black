using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StageProgress : MonoBehaviour
{
    [SerializeField]
    int progressInt;

    [SerializeField]
    Image progressBar;

    [SerializeField]
    StageProgressDot[] stageProgressDotList;

    const int ProgressStep = 5;

    [SerializeField]
    Color activeColor;

    [SerializeField]
    Color inactiveColor;

    public int ProgressInt
    {
        get => progressInt;
        set
        {
            progressInt = value;
            UpdateProgress();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
        UpdateProgress();
    }
#endif

    void UpdateProgress()
    {
        progressInt = Mathf.Clamp(progressInt, 0, stageProgressDotList.Length);
        
        for (var i = 0; i < stageProgressDotList.Length; i++)
        {
            var dot = stageProgressDotList[i];
            dot.SetColor(i < progressInt ? activeColor : inactiveColor);
            stageProgressDotList[i].SetAnimActive(i == progressInt);
        }
        
        progressBar.fillAmount = (float) progressInt / ProgressStep;
    }
}