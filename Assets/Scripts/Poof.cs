using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Poof : MonoBehaviour
{
    Action finishAction;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    public void ExecuteFinishAction()
    {
        finishAction?.Invoke();
    }

    public void SetFinishAction(Action action)
    {
        finishAction = action;
    }
}