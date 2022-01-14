using ConditionalDebug;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StageLocker : MonoBehaviour
{
    [SerializeField]
    Subcanvas subcanvas;

    [SerializeField]
    Image gauge;

    [SerializeField]
    Text text;

    [SerializeField]
    float initialTime;

    [SerializeField]
    float remainTime;

    [SerializeField]
    bool unlockForce;

    public bool Locked => enabled && initialTime > 0 && remainTime > 0;

    public float RemainTime
    {
        get => Locked ? remainTime : 0;
        set
        {
            remainTime = value;
            if (remainTime > 0)
            {
                Lock();
            }
        }
    }

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
        var oldStageLockRemainTime = remainTime;
        remainTime = Mathf.Clamp(remainTime - Time.deltaTime, 0, initialTime);

        if (initialTime <= 0 || remainTime <= 0 || unlockForce)
        {
            ChangeToUnlocked();
        }
        else
        {
            ChangeToLocked(oldStageLockRemainTime);
        }
    }

    void ChangeToLocked(float oldStageLockRemainTime)
    {
        if (oldStageLockRemainTime <= 0 && remainTime > 0)
        {
            OnStageLocked?.Invoke();
        }

        subcanvas.Open();
        gauge.fillAmount = remainTime / initialTime;
        text.text = @"\다음 스테이지 준비중\n{0:F1}초".Localized(remainTime);
        unlockForce = false;
    }

    void ChangeToUnlocked()
    {
        subcanvas.Close();

        OnStageUnlocked?.Invoke();

        enabled = false;

        unlockForce = false;
    }

    [UsedImplicitly]
    void OpenPopup()
    {
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }

    public void UnlockWithRemainTimeReset()
    {
        ConDebug.LogTrace();
        remainTime = 0;
        unlockForce = false;
    }

    public void UnlockWithoutRemainTimeReset()
    {
        unlockForce = true;
    }

    public void Lock()
    {
        enabled = true;
    }
}