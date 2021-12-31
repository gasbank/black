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

    public bool Locked => enabled && initialTime > 0 && remainTime > 0;

    public float RemainTime
    {
        get => Locked ? remainTime : 0;
        set
        {
            remainTime = value;
            if (remainTime > 0)
            {
                enabled = true;
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

        if (initialTime <= 0 || remainTime <= 0)
        {
            subcanvas.Close();

            OnStageUnlocked?.Invoke();

            enabled = false;
        }
        else
        {
            if (oldStageLockRemainTime <= 0 && remainTime > 0)
            {
                OnStageLocked?.Invoke();
            }

            subcanvas.Open();
            gauge.fillAmount = remainTime / initialTime;
            text.text = @"\다음 스테이지 준비중\n{0:F1}초".Localized(remainTime);
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

    public void Unlock()
    {
        remainTime = 0;
    }
}