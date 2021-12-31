using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StageLocker : MonoBehaviour
{
    [SerializeField]
    Subcanvas subcanvas;

    [SerializeField]
    Image stageLockerGauge;

    [SerializeField]
    Text stageLockerText;

    [SerializeField]
    float stageLockInitialTime;

    [SerializeField]
    float stageLockRemainTime;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    
    public event NotifyStageUnlock OnStageUnlocked;
    public event NotifyStageLock OnStageLocked;

    void Update()
    {
        var oldStageLockRemainTime = stageLockRemainTime;
        stageLockRemainTime = Mathf.Clamp(stageLockRemainTime - Time.deltaTime, 0, stageLockInitialTime);

        if (stageLockInitialTime <= 0 || (oldStageLockRemainTime > 0 && stageLockRemainTime <= 0))
        {
            subcanvas.Close();

            OnStageUnlocked?.Invoke();

            enabled = false;
        }
        else
        {
            if (oldStageLockRemainTime <= 0 && stageLockRemainTime > 0)
            {
                OnStageLocked?.Invoke();
            }

            subcanvas.Open();
            stageLockerGauge.fillAmount = stageLockRemainTime / stageLockInitialTime;
            stageLockerText.text = @"\다음 스테이지 준비중\n{0:F1}초".Localized(stageLockRemainTime);
        }
    }

    [UsedImplicitly]
    void OpenPopup()
    {
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}