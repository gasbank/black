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
    Image[] progressDotList;

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
        progressInt = Mathf.Clamp(progressInt, 0, progressDotList.Length);
        
        for (var i = 0; i < progressDotList.Length; i++)
        {
            var dot = progressDotList[i];
            dot.color = i < progressInt ? activeColor : inactiveColor;
        }

        progressBar.fillAmount = (float) progressInt / ProgressStep;
    }
}